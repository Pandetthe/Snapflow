using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetById;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Boards;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}", async (
            int boardId,
            IQueryHandler<GetBoardByIdQuery, BoardResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBoardByIdQuery(boardId);
            Result<BoardResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Boards)
        .Produces<BoardResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesInternalServerError();
    }
}
