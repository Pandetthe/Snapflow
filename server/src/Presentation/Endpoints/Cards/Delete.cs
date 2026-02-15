using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Delete;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Cards;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}/cards/{cardId:int}", async (
            int boardId, int cardId,
            ICommandHandler<DeleteCardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteCardCommand(cardId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Cards.Delete)
        .WithTags(EndpointTags.Cards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}