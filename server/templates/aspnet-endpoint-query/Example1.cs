using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Caching;
using Snapflow.Presentation.Extensions;

namespace SnapflowAspNet;

internal sealed class Example1 : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/example1", async (
            int boardId,
            IQueryHandler<Example1Query, Example1Response> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new Example1Query(boardId);

            Result<Example1Response> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .CacheOutput(CachePolicies.Board)
        .WithTags(EndpointTags.Boards)
        .Produces<Example1Response>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
