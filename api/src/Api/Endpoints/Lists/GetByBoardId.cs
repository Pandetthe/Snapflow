using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.GetByBoardId;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/lists", async (
            int boardId,
            IQueryHandler<GetListsByBoardIdQuery, ListsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListsByBoardIdQuery(boardId);

            Result<ListsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Lists);
    }
}