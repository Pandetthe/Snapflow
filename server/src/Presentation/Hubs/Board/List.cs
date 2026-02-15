using Microsoft.AspNetCore.Authorization;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Create;
using Snapflow.Application.Lists.Delete;
using Snapflow.Application.Lists.Move;
using Snapflow.Application.Lists.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Hubs.Board;

// Hub methods for list operations
public sealed partial class BoardHub
{
    public sealed record CreateListRequest(int SwimlaneId, string Title, int? Width, int? BeforeId);

    [Authorize(BoardPermissions.Lists.Create)]
    public async Task<IResult> CreateList(
        CreateListRequest request,
        ICommandHandler<CreateListCommand, int> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("List create requested by connection {ConnectionId}.", Context.ConnectionId);
        }
        var command = new CreateListCommand(request.SwimlaneId, request.Title, request.Width, request.BeforeId);
        Result<int> result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.OkWithId, Results.Problem);
    }

    public sealed record UpdateListRequest(int Id, string Title, int? Width);

    [Authorize(BoardPermissions.Lists.Update)]
    public async Task<IResult> UpdateList(
        UpdateListRequest request,
        ICommandHandler<UpdateListCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("List update requested by connection {ConnectionId}.", Context.ConnectionId);
        }
        var command = new UpdateListCommand(request.Id, request.Title, request.Width);
        Result result = await handler.Handle(command);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record MoveListRequest(int Id, int SwimlaneId, int? BeforeId);

    [Authorize(BoardPermissions.Lists.Move)]
    public async Task<IResult> MoveList(
        MoveListRequest request,
        ICommandHandler<MoveListCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("List move requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new MoveListCommand(request.Id, request.SwimlaneId, request.BeforeId);
        Result result = await handler.Handle(command);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record DeleteListRequest(int Id);

    [Authorize(BoardPermissions.Lists.Delete)]
    public async Task<IResult> DeleteList(
        DeleteListRequest request,
        ICommandHandler<DeleteListCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("List delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteListCommand(request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }
}