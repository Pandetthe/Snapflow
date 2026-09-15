using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.TwoFactorPasskeyOptions;
using Snapflow.Presentation.Contracts;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class TwoFactorPasskeyOptions : IEndpoint
{
    public sealed record TwoFactorPasskeyOptionsRequest(string? TwoFactorToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-in/two-factor/passkey/options", async (
            TwoFactorPasskeyOptionsRequest? request,
            ICommandHandler<TwoFactorPasskeyOptionsCommand, PasskeyChallenge> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new TwoFactorPasskeyOptionsCommand(request?.TwoFactorToken);

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(challenge => Results.Ok(PasskeyOptionsResponse.From(challenge)), Results.Problem);
        })
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Auth)
        .Produces<PasskeyOptionsResponse>();
    }
}
