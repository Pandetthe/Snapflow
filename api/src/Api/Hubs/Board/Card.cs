using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Delete;
using Snapflow.Application.Cards.Update;
using Snapflow.Common;

namespace Snapflow.Api.Hubs.Board;

// Hub methods for card operations
internal sealed partial class BoardHub
{
    public sealed record CreateCardRequest(int SwimlaneId, int ListId, string Title, string Description);

    public async Task<IResult> CreateCard(
        CreateCardRequest request,
        ICommandHandler<UpdateCardCommand> handler)
    {
        logger.LogInformation("Card create requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new UpdateCardCommand(
            Context.GetBoardId(),
            request.Title,
            request.Description);
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public sealed record UpdateCardRequest();

    public Task UpdateCard(
        UpdateCardRequest request)
    {
        logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public sealed record MoveCardRequest();

    public Task MoveCard(
        MoveCardRequest request)
    {
        logger.LogInformation("Card update requested by connection {ConnectionId}.", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public sealed record DeleteCardRequest(int Id, int SwimlaneId, int ListId);

    public async Task<IResult> DeleteCard(
        DeleteCardRequest request,
        ICommandHandler<DeleteCardCommand> handler)
    {
        logger.LogInformation("Card delete requested by connection {ConnectionId}.", Context.ConnectionId);
        var command = new DeleteCardCommand(
            request.Id,
            Context.GetBoardId());
        Result result = await handler.Handle(command, Context.ConnectionAborted);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
