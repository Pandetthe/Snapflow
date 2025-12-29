using Microsoft.AspNetCore.Authorization;
using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Create;
using Snapflow.Application.Cards.Delete;
using Snapflow.Application.Cards.Move;
using Snapflow.Application.Cards.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Hubs.Board;

// Hub methods for card operations
public sealed partial class BoardHub
{
    public sealed record CreateCardRequest(int ListId, string Title, string Description, int? BeforeId);

    [Authorize(BoardPermissions.Cards.Create)]
    public async Task<IResult> CreateCard(
        CreateCardRequest request,
        ICommandHandler<CreateCardCommand, int> handler)
    {
        logger.LogInformation("Card create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new CreateCardCommand(
            request.ListId,
            request.Title,
            request.Description,
            request.BeforeId);
        Result<int> result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(CustomResults.OkWithId, CustomResults.Problem);
    }

    public sealed record UpdateCardRequest(int Id, string Title, string Description);

    [Authorize(BoardPermissions.Cards.Update)]
    public async Task<IResult> UpdateCard(
        UpdateCardRequest request,
        ICommandHandler<UpdateCardCommand> handler)
    {
        logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new UpdateCardCommand(
            request.Id,
            request.Title,
            request.Description);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public sealed record MoveCardRequest(int Id, int ListId, int? BeforeId);

    [Authorize(BoardPermissions.Cards.Move)]
    public async Task<IResult> MoveCard(
        MoveCardRequest request,
        ICommandHandler<MoveCardCommand> handler)
    {
        logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new MoveCardCommand(request.Id, request.ListId, request.BeforeId);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public sealed record DeleteCardRequest(int Id);

    [Authorize(BoardPermissions.Cards.Delete)]
    public async Task<IResult> DeleteCard(
        DeleteCardRequest request,
        ICommandHandler<DeleteCardCommand> handler)
    {
        logger.LogInformation("Card delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteCardCommand(request.Id);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
