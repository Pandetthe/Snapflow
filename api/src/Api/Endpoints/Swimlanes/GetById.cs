using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.GetById;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes/{swimlaneId:int}", async (
            int boardId, int swimlaneId,
            IQueryHandler<GetSwimlaneByIdQuery, GetSwimlaneByIdResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSwimlaneByIdQuery(swimlaneId);

            Result<GetSwimlaneByIdResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Swimlanes)
        .Produces<GetSwimlaneByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
