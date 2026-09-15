using Microsoft.AspNetCore.Authorization;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Tags.AddToCard;
using Snapflow.Application.Tags.Create;
using Snapflow.Application.Tags.Delete;
using Snapflow.Application.Tags.RemoveFromCard;
using Snapflow.Application.Tags.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Hubs.Board;

// Hub methods for tag operations
public sealed partial class BoardHub
{
    public sealed record CreateTagRequest(string Title, TagColors Color);

    [Authorize(BoardPermissions.Tags.Create)]
    public async Task<IResult> CreateTag(
        CreateTagRequest request,
        ICommandHandler<CreateTagCommand, CreateTagResponse> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Tag create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateTagCommand(Context.GetBoardId(), request.Title, request.Color);
        var result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.Ok, Results.Problem);
    }

    public sealed record UpdateTagRequest(int Id, string Title, TagColors Color);

    [Authorize(BoardPermissions.Tags.Update)]
    public async Task<IResult> UpdateTag(
        UpdateTagRequest request,
        ICommandHandler<UpdateTagCommand, UpdateTagResponse> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Tag update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new UpdateTagCommand(Context.GetBoardId(), request.Id, request.Title, request.Color);
        var result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.Ok, Results.Problem);
    }

    public sealed record DeleteTagRequest(int Id);

    [Authorize(BoardPermissions.Tags.Delete)]
    public async Task<IResult> DeleteTag(
        DeleteTagRequest request,
        ICommandHandler<DeleteTagCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Tag delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteTagCommand(Context.GetBoardId(), request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record AddTagToCardRequest(int CardId, int TagId);

    [Authorize(BoardPermissions.Tags.Assign)]
    public async Task<IResult> AddTagToCard(
        AddTagToCardRequest request,
        ICommandHandler<AddTagToCardCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Card tag add requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new AddTagToCardCommand(Context.GetBoardId(), request.CardId, request.TagId);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record RemoveTagFromCardRequest(int CardId, int TagId);

    [Authorize(BoardPermissions.Tags.Assign)]
    public async Task<IResult> RemoveTagFromCard(
        RemoveTagFromCardRequest request,
        ICommandHandler<RemoveTagFromCardCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Card tag remove requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new RemoveTagFromCardCommand(Context.GetBoardId(), request.CardId, request.TagId);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }
}
