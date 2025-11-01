using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Create;
using Snapflow.Application.Swimlanes.Delete;
using Snapflow.Common;

namespace Snapflow.Api.Hubs.Board;

// Hub methods for swimlane operations
internal sealed partial class BoardHub
{
    public sealed record CreateSwimlaneRequest(string Title);

    public async Task<IResult> CreateSwimlane(
        CreateSwimlaneRequest request,
        ICommandHandler<CreateSwimlaneCommand, int> handler)
    {
        logger.LogInformation("Swimlane create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateSwimlaneCommand(Context.GetBoardId(), request.Title);
        Result<int> result = await handler.Handle(command);
        return result.Match(CustomResults.OkWithId, CustomResults.Problem);
    }

    public Task UpdateSwimlane()
    {
        logger.LogInformation("Swimlane update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task MoveSwimlane()
    {
        logger.LogInformation("Swimlane update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public sealed record DeleteSwimlaneRequest(int Id);

    public async Task<IResult> DeleteSwimlane(
        DeleteSwimlaneRequest request,
        ICommandHandler<DeleteSwimlaneCommand> handler)
    {
        logger.LogInformation("Swimlane delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteSwimlaneCommand(request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}