using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.GetVisibilityOptions;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class GetVisibilityOptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/visibility-options", async (
            IQueryHandler<GetBoardVisibilityOptionsQuery, GetBoardVisibilityOptionsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<GetBoardVisibilityOptionsResponse> result =
                await handler.Handle(new GetBoardVisibilityOptionsQuery(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Boards)
        .Produces<GetBoardVisibilityOptionsResponse>(StatusCodes.Status200OK);
    }
}
