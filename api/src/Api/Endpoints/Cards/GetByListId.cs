using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetByListId;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetByListId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/lists/{listId:int}/cards", async (
            int boardId, int listId,
            IQueryHandler<GetCardsByListIdQuery, CardsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardsByListIdQuery(listId);

            Result<CardsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Cards);
    }
}