using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Tags.RemoveFromCard;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Tags;

internal sealed class RemoveFromCard : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}/cards/{cardId:int}/tags/{tagId:int}", async (
            int boardId, int cardId, int tagId,
            ICommandHandler<RemoveTagFromCardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveTagFromCardCommand(boardId, cardId, tagId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Tags.Assign)
        .WithTags(EndpointTags.Tags)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
