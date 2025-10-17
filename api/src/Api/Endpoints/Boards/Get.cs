﻿using Microsoft.AspNetCore.Mvc;
using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Get;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Boards;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards", async (
            [FromQuery] string? title,
            IQueryHandler<GetBoardsQuery, List<BoardResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBoardsQuery(title);
            Result<List<BoardResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Boards)
        .RequireAuthorization();
    }
}