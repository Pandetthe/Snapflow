using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetByBoardId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Cards.GetByBoardId.GetCardsByBoardIdResponse;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/cards", async (
            int boardId,
            IQueryHandler<GetCardsByBoardIdQuery, IReadOnlyList<CardDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardsByBoardIdQuery(boardId);

            Result<IReadOnlyList<CardDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Cards)
        .Produces<GetCardsByBoardIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}