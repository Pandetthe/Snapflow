using Snapflow.Presentation.Caching;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetContent;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class GetContent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/content", async (
            int boardId,
            IQueryHandler<GetBoardContentQuery, GetBoardContentResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBoardContentQuery(boardId);
            Result<GetBoardContentResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .CacheOutput(CachePolicies.Board)
        .WithTags(EndpointTags.Boards)
        .Produces<GetBoardContentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
