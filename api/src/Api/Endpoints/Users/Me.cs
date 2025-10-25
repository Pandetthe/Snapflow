using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Users;

internal sealed class SignUp : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (
            IQueryHandler<MeQuery, MeResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<MeResponse> result = await handler.Handle(new(), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}