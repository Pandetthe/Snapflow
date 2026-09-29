using Snapflow.Presentation.Caching;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Members.Get;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class GetMembers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/members", async (
            int boardId,
            IQueryHandler<GetMembersQuery, IReadOnlyList<GetMembersResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMembersQuery(boardId);

            Result<IReadOnlyList<GetMembersResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .CacheOutput(CachePolicies.Board)
        .WithTags(EndpointTags.Boards)
        .Produces<IReadOnlyList<GetMembersResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
