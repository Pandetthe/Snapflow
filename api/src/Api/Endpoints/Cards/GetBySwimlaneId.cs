using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetBySwimlaneId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Cards.GetBySwimlaneId.GetCardsBySwimlaneIdResponse;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetBySwimlaneId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes/{swimlaneId:int}/cards", async (
            int boardId, int swimlaneId,
            IQueryHandler<GetCardsBySwimlaneIdQuery, IReadOnlyList<CardDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardsBySwimlaneIdQuery(swimlaneId);

            Result<IReadOnlyList<CardDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Cards)
        .Produces<IReadOnlyList<CardDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}