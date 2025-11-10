using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Create;
using Snapflow.Application.Lists.Delete;
using Snapflow.Application.Lists.Move;
using Snapflow.Application.Lists.Update;
using Snapflow.Common;

namespace Snapflow.Api.Hubs.Board;

// Hub methods for list operations
internal sealed partial class BoardHub
{
    public sealed record CreateListRequest(int SwimlaneId, string Title, int? BeforeId);

    public async Task<IResult> CreateList(
        CreateListRequest request,
        ICommandHandler<CreateListCommand, int> handler)
    {
        logger.LogInformation("List create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateListCommand(request.SwimlaneId, request.Title, request.BeforeId);
        Result<int> result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(CustomResults.OkWithId, CustomResults.Problem);
    }

    public sealed record UpdateListRequest(int Id, string Title);

    public async Task<IResult> UpdateList(
        UpdateListRequest request,
        ICommandHandler<UpdateListCommand> handler)
    {
        logger.LogInformation("List update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new UpdateListCommand(request.Id, request.Title);
        Result result = await handler.Handle(command);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public sealed record MoveListRequest(int Id, int? BeforeId);

    public async Task<IResult> MoveList(
        MoveListRequest request,
        ICommandHandler<MoveListCommand> handler)
    {
        logger.LogInformation("List move requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new MoveListCommand(request.Id, request.BeforeId);
        Result result = await handler.Handle(command);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public sealed record DeleteListRequest(int Id);

    public async Task<IResult> DeleteList(
        DeleteListRequest request,
        ICommandHandler<DeleteListCommand> handler)
    {
        logger.LogInformation("List delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteListCommand(request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}