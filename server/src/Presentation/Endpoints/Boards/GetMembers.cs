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
            IQueryHandler<GetMembersQuery, List<GetMembersResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMembersQuery(boardId);

            Result<List<GetMembersResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Boards)
        .Produces<List<GetMembersResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
