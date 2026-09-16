using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetPublic;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class GetPublic : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/public", async (
            IQueryHandler<GetPublicBoardsQuery, IReadOnlyList<GetPublicBoardsResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<IReadOnlyList<GetPublicBoardsResponse>> result =
                await handler.Handle(new GetPublicBoardsQuery(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .AllowAnonymous()
        .WithTags(EndpointTags.Boards)
        .Produces<IReadOnlyList<GetPublicBoardsResponse>>(StatusCodes.Status200OK);
    }
}
