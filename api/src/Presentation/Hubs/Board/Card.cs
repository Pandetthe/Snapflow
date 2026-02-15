using Microsoft.AspNetCore.Authorization;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Create;
using Snapflow.Application.Cards.Delete;
using Snapflow.Application.Cards.Move;
using Snapflow.Application.Cards.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Hubs.Board;

// Hub methods for card operations
public sealed partial class BoardHub
{
    public sealed record CreateCardRequest(int ListId, string Title, string Description, int? BeforeId);

    [Authorize(BoardPermissions.Cards.Create)]
    public async Task<IResult> CreateCard(
        CreateCardRequest request,
        ICommandHandler<CreateCardCommand, int> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Card create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateCardCommand(
            request.ListId,
            request.Title,
            request.Description,
            request.BeforeId);
        Result<int> result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.OkWithId, Results.Problem);
    }

    public sealed record UpdateCardRequest(int Id, string Title, string Description);

    [Authorize(BoardPermissions.Cards.Update)]
    public async Task<IResult> UpdateCard(
        UpdateCardRequest request,
        ICommandHandler<UpdateCardCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new UpdateCardCommand(
            request.Id,
            request.Title,
            request.Description);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record MoveCardRequest(int Id, int ListId, int? BeforeId);

    [Authorize(BoardPermissions.Cards.Move)]
    public async Task<IResult> MoveCard(
        MoveCardRequest request,
        ICommandHandler<MoveCardCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new MoveCardCommand(request.Id, request.ListId, request.BeforeId);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }

    public sealed record DeleteCardRequest(int Id);

    [Authorize(BoardPermissions.Cards.Delete)]
    public async Task<IResult> DeleteCard(
        DeleteCardRequest request,
        ICommandHandler<DeleteCardCommand> handler)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Card delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteCardCommand(request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, Results.Problem);
    }
}
