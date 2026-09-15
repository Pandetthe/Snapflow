using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.PasskeySignInOptions;
using Snapflow.Presentation.Contracts;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class PasskeySignInOptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-in/passkey/options", async (
            ICommandHandler<PasskeySignInOptionsCommand, PasskeyChallenge> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new PasskeySignInOptionsCommand(), cancellationToken);

            return result.Match(challenge => Results.Ok(PasskeyOptionsResponse.From(challenge)), Results.Problem);
        })
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Auth)
        .Produces<PasskeyOptionsResponse>();
    }
}
