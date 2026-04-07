using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Search;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users;

internal sealed class Search : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/search", async (
            [FromQuery] string name,
            [FromQuery] int[] excludedIds,
            IQueryHandler<SearchUsersQuery, IReadOnlyList<SearchUsersResponse.UserDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchUsersQuery(name, excludedIds.ToHashSet());
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<IReadOnlyList<SearchUsersResponse.UserDto>>()
        .ProducesValidationProblem();
    }
}
