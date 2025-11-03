using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetBySwimlaneId;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetBySwimlaneId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes/{swimlaneId:int}/cards", async (
            int boardId, int swimlaneId,
            IQueryHandler<GetCardsBySwimlaneIdQuery, CardsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardsBySwimlaneIdQuery(swimlaneId);

            Result<CardsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Cards);
    }
}