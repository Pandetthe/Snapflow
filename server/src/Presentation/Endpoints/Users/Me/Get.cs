using Snapflow.Presentation.Caching;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.GetMe;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me", async (
            IQueryHandler<MeQuery, MeResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new MeQuery();

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .CacheOutput(CachePolicies.User)
        .WithTags(EndpointTags.Users)
        .Produces<MeResponse>();
    }
}