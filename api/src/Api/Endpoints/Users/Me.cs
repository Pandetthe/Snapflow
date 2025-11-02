using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Users;

internal sealed class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me", async (
            IQueryHandler<MeQuery, MeResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new MeQuery();

            Result<MeResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users);
    }
}