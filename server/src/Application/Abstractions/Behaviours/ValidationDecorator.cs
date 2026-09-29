using FluentValidation;
using FluentValidation.Results;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Abstractions.Behaviours;

internal static class ValidationDecorator
{
    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        IEnumerable<IValidator<TQuery>> validators)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken) =>
            await ValidateAsync(query, validators, cancellationToken)
            ?? await innerHandler.Handle(query, cancellationToken);
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken) =>
            await ValidateAsync(command, validators, cancellationToken)
            ?? await innerHandler.Handle(command, cancellationToken);
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken) =>
            await ValidateAsync(command, validators, cancellationToken)
            ?? await innerHandler.Handle(command, cancellationToken);
    }

    private static async Task<ValidationError?> ValidateAsync<TRequest>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators,
        CancellationToken cancellationToken)
    {
        IValidator<TRequest>[] all = [.. validators];
        if (all.Length == 0)
            return null;

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            all.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        PropertyValidationError[] errors =
        [
            .. results
                .SelectMany(result => result.Errors)
                .Select(f => new PropertyValidationError(f.PropertyName, f.ErrorCode, f.ErrorMessage))
        ];

        return errors.Length == 0 ? null : new ValidationError(errors);
    }
}
