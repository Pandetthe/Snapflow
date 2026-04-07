using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetDetails;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class GetDetails : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/details", async (
            int boardId,
            IQueryHandler<GetBoardDetailsQuery, GetBoardDetailsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBoardDetailsQuery(boardId);
            Result<GetBoardDetailsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Boards)
        .Produces<GetBoardDetailsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
