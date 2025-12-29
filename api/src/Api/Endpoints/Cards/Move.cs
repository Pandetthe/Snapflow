using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Move;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class Move : IEndpoint
{
    public sealed record MoveCardRequest(int ListId, int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/cards/{cardId:int}/move", async (
            MoveCardRequest request,
            int boardId, int cardId,
            ICommandHandler<MoveCardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MoveCardCommand(cardId, request.ListId, request.BeforeId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Cards.Move)
        .WithTags(EndpointTags.Cards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}