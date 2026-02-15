using Microsoft.AspNetCore.Authorization;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Create;
using Snapflow.Application.Swimlanes.Delete;
using Snapflow.Application.Swimlanes.Move;
using Snapflow.Application.Swimlanes.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Hubs.Board;

// Hub methods for swimlane operations
public sealed partial class BoardHub
{
    public sealed record CreateSwimlaneRequest(string Title, int? Height, int? BeforeId);

    [Authorize(BoardPermissions.Swimlanes.Create)]
    public async Task<IResult> CreateSwimlane(
        CreateSwimlaneRequest request,
        ICommandHandler<CreateSwimlaneCommand, int> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Swimlane create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateSwimlaneCommand(Context.GetBoardId(), request.Title, request.Height, request.BeforeId);
        Result<int> result = await handler.Handle(command);
        return result.Match(Results.OkWithId, Results.Problem);
    }

    public sealed record UpdateSwimlaneRequest(int Id, string Title, int? Height);

    [Authorize(BoardPermissions.Swimlanes.Update)]
    public async Task<IResult> UpdateSwimlane(
        UpdateSwimlaneRequest request,
        ICommandHandler<UpdateSwimlaneCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Swimlane update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new UpdateSwimlaneCommand(request.Id, request.Title, request.Height);
        Result result = await handler.Handle(command);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record MoveSwimlaneRequest(int Id, int? BeforeId);

    [Authorize(BoardPermissions.Swimlanes.Move)]
    public async Task<IResult> MoveSwimlane(
        MoveSwimlaneRequest request,
        ICommandHandler<MoveSwimlaneCommand, string> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Swimlane move requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new MoveSwimlaneCommand(request.Id, request.BeforeId);
        Result<string> result = await handler.Handle(command);
        return result.Match(Results.OkWithRank, Results.Problem);
    }

    public sealed record DeleteSwimlaneRequest(int Id);

    [Authorize(BoardPermissions.Swimlanes.Delete)]
    public async Task<IResult> DeleteSwimlane(
        DeleteSwimlaneRequest request,
        ICommandHandler<DeleteSwimlaneCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Swimlane delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteSwimlaneCommand(request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }
}