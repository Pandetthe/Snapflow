using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.GetByBoardId;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Swimlanes.GetByBoardId.GetSwimlanesByBoardIdResponse;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class GetByBoardId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes", async (
            int boardId,
            IQueryHandler<GetSwimlanesByBoardIdQuery, IReadOnlyList<SwimlaneDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSwimlanesByBoardIdQuery(boardId);

            Result<IReadOnlyList<SwimlaneDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Swimlanes)
        .Produces<IReadOnlyList<SwimlaneDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}