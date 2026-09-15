using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.PasskeySignIn;
using Snapflow.Common;
using Snapflow.Presentation.Contracts;
using Snapflow.Presentation.Extensions;
using System.Text.Json;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class PasskeySignIn : IEndpoint
{
    public sealed record PasskeySignInRequest(JsonElement? Credential, string State);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-in/passkey", async (
            PasskeySignInRequest request,
            bool? useCookies,
            bool? useSessionCookies,
            ICommandHandler<PasskeySignInCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new PasskeySignInCommand(
                PasskeyCredentialJson.From(request.Credential) ?? string.Empty,
                request.State,
                useCookies,
                useSessionCookies);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Empty, Results.Problem);
        })
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Auth);
    }
}
