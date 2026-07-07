using Microsoft.Extensions.Logging;
using Serilog.Context;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Abstractions.Behaviours;

internal static class LoggingDecorator
{
    private static async Task<TResult> HandleWithLogging<TResult>(
        string kind,
        string name,
        ILogger logger,
        Func<Task<TResult>> innerHandle)
        where TResult : Result
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Processing {Kind} {Name}", kind, name);

        TResult result = await innerHandle();

        if (!logger.IsEnabled(LogLevel.Information)) return result;
        if (result.IsSuccess)
            logger.LogInformation("Completed {Kind} {Name}", kind, name);
        else
        {
            using (LogContext.PushProperty("Error", result.Error, true))
                logger.LogInformation("Completed {Kind} {Name} with error", kind, name);
        }
        return result;
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken) =>
            HandleWithLogging("command", typeof(TCommand).Name, logger,
                () => innerHandler.Handle(command, cancellationToken));
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public Task<Result> Handle(TCommand command, CancellationToken cancellationToken) =>
            HandleWithLogging("command", typeof(TCommand).Name, logger,
                () => innerHandler.Handle(command, cancellationToken));
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken) =>
            HandleWithLogging("query", typeof(TQuery).Name, logger,
                () => innerHandler.Handle(query, cancellationToken));
    }
}
