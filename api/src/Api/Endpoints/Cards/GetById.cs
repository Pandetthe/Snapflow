using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetById;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/cards/{cardId:int}", async (
            int boardId, int cardId,
            IQueryHandler<GetCardByIdQuery, CardResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardByIdQuery(cardId);

            Result<CardResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Cards);
    }
}
