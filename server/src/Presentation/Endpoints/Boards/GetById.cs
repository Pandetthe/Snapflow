using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetById;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}", async (
            int boardId,
            IQueryHandler<GetBoardByIdQuery, GetBoardByIdResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBoardByIdQuery(boardId);
            Result<GetBoardByIdResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Boards)
        .Produces<GetBoardByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
