using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Delete;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Cards;

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

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Cards);
    }
}