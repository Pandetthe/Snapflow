using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.SignIn;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class SignIn : IEndpoint
{
    public sealed record SignInRequest(string Email, string Password, string? TwoFactorCode, string? TwoFactorRecoveryCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-in", async (
            SignInRequest request,
            bool? useCookies,
            bool? useSessionCookies,
            ICommandHandler<SignInCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new SignInCommand(
                request.Email,
                request.Password,
                request.TwoFactorCode,
                request.TwoFactorRecoveryCode,
                useCookies,
                useSessionCookies);

            Result result = await handler.Handle(command, cancellationToken);

            // Application returns data via asp.net authentication mechanisms
            return result.Match(Results.Empty, Results.Problem);
        })
        .WithTags(EndpointTags.Auth);
    }
}