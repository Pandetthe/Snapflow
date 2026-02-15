using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.GetById;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Cards;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/cards/{cardId:int}", async (
            int boardId, int cardId,
            IQueryHandler<GetCardByIdQuery, GetCardByIdResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCardByIdQuery(cardId);

            Result<GetCardByIdResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Cards)
        .Produces<GetCardByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
