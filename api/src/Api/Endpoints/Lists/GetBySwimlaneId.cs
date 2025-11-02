using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.GetBySwimlaneId;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class GetBySwimlaneId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes/{swimlaneId:int}/lists", async (
            int boardId, int swimlaneId,
            IQueryHandler<GetListsBySwimlaneIdQuery, ListsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListsBySwimlaneIdQuery(swimlaneId);

            Result<ListsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Lists);
    }
}