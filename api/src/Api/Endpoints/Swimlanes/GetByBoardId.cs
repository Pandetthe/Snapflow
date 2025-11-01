using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.GetByBoardId;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes", async (
            int boardId,
            IQueryHandler<GetSwimlaneByBoardIdQuery, List<GetSwimlaneByBoardIdResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSwimlaneByBoardIdQuery(boardId);

            Result<List<GetSwimlaneByBoardIdResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Swimlanes);
    }
}