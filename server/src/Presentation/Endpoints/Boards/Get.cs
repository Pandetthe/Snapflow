using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Get;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards", async (
            [FromQuery] string? title,
            IQueryHandler<GetBoardsQuery, IReadOnlyList<GetBoardsResponse.BoardDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBoardsQuery(title);
            Result<IReadOnlyList<GetBoardsResponse.BoardDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Boards)
        .Produces<IReadOnlyList<GetBoardsResponse.BoardDto>>(StatusCodes.Status200OK);
    }
}