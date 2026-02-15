using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users;

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

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<MeResponse>(StatusCodes.Status200OK);
    }
}