using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Cards;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateCardRequest(string Title, string Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/cards/{cardId:int}", async (
            UpdateCardRequest request,
            int boardId, int cardId,
            ICommandHandler<UpdateCardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCardCommand(cardId, request.Title, request.Description);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Cards.Update)
        .WithTags(EndpointTags.Cards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}