using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.TwoFactorSignIn;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class TwoFactorSignIn : IEndpoint
{
    public sealed record TwoFactorSignInRequest(string? Code, string? RecoveryCode, bool RememberDevice, string? TwoFactorToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-in/two-factor", async (
            TwoFactorSignInRequest request,
            bool? useCookies,
            bool? useSessionCookies,
            ICommandHandler<TwoFactorSignInCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new TwoFactorSignInCommand(
                request.Code,
                request.RecoveryCode,
                request.RememberDevice,
                request.TwoFactorToken,
                useCookies,
                useSessionCookies);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Empty, Results.Problem);
        })
        .WithTags(EndpointTags.Auth);
    }
}
