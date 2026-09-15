using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Tags.GetByBoardId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Caching;
using Snapflow.Presentation.Extensions;
using static Snapflow.Application.Tags.GetByBoardId.GetTagsByBoardIdResponse;

namespace Snapflow.Presentation.Endpoints.Tags;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/tags", async (
            int boardId,
            IQueryHandler<GetTagsByBoardIdQuery, IReadOnlyList<TagDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTagsByBoardIdQuery(boardId);

            Result<IReadOnlyList<TagDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .CacheOutput(CachePolicies.Board)
        .WithTags(EndpointTags.Tags)
        .Produces<IReadOnlyList<TagDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
