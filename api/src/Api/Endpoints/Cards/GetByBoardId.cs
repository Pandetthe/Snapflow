using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetByBoardId;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/cards", async (
            int boardId,
            IQueryHandler<GetCardsByBoardIdQuery, CardsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardsByBoardIdQuery(boardId);

            Result<CardsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Cards);
    }
}