using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.GetBySwimlaneId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Lists.GetBySwimlaneId.GetListsBySwimlaneIdResponse;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class GetBySwimlaneId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes/{swimlaneId:int}/lists", async (
            int boardId, int swimlaneId,
            IQueryHandler<GetListsBySwimlaneIdQuery, IReadOnlyList<ListDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListsBySwimlaneIdQuery(swimlaneId);

            Result<IReadOnlyList<ListDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Lists)
        .Produces<IReadOnlyList<ListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}