using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Passkeys.GetAll;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class GetPasskeys : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/passkeys", async (
            IQueryHandler<GetPasskeysQuery, IReadOnlyList<PasskeyDetails>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new GetPasskeysQuery(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces<IReadOnlyList<PasskeyDetails>>();
    }
}
