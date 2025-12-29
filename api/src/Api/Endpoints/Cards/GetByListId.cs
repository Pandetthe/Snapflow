using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetByListId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Cards.GetByListId.GetCardsByListIdResponse;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class GetByListId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/lists/{listId:int}/cards", async (
            int boardId, int listId,
            IQueryHandler<GetCardsByListIdQuery, IReadOnlyList<CardDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardsByListIdQuery(listId);

            Result<IReadOnlyList<CardDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Cards)
        .Produces<IReadOnlyList<CardDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}