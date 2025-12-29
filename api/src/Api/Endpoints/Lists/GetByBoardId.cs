using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.GetByBoardId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Lists.GetByBoardId.GetListsByBoardId;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/lists", async (
            int boardId,
            IQueryHandler<GetListsByBoardIdQuery, IReadOnlyList<ListDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListsByBoardIdQuery(boardId);

            Result<IReadOnlyList<ListDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Lists)
        .Produces<IReadOnlyList<ListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}