using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Create;
using Snapflow.Application.Lists.Delete;
using Snapflow.Common;

namespace Snapflow.Api.Hubs.Board;

// Hub methods for list operations
internal sealed partial class BoardHub
{
    public sealed record CreateListRequest(int SwimlaneId, string Title);

    public async Task<IResult> CreateList(
        CreateListRequest request,
        ICommandHandler<CreateListCommand, int> handler)
    {
        logger.LogInformation("List create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateListCommand(Context.GetBoardId(), request.SwimlaneId, request.Title);
        Result<int> result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(CustomResults.OkWithId, CustomResults.Problem);
    }

    public Task UpdateList()
    {
        logger.LogInformation("List update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public Task MoveList()
    {
        logger.LogInformation("List update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public sealed record DeleteListRequest(int SwimlaneId, int ListId);

    public async Task<IResult> DeleteList(
        DeleteListRequest request,
        ICommandHandler<DeleteListCommand> handler)
    {
        logger.LogInformation("List delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteListCommand(request.ListId, Context.GetBoardId(), request.SwimlaneId);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}