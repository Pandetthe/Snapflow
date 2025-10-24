using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.Login;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class Login : IEndpoint
{
    public sealed class LoginRequest
    {
        public required string Email { get; init; }

        public required string Password { get; init; }

        public string? TwoFactorCode { get; init; }

        public string? TwoFactorRecoveryCode { get; init; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            LoginRequest request,
            ICommandHandler<LoginCommand> handler,
            bool? useCookies,
            bool? useSessionCookies,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCommand(
                request.Email,
                request.Password,
                request.TwoFactorCode,
                request.TwoFactorRecoveryCode,
                useCookies,
                useSessionCookies);

            Result result = await handler.Handle(command, cancellationToken);
            if (result.IsFailure)
                return CustomResults.Problem(result);
            return Results.Empty; // Application returns data via asp.net authentication mechanisms
        })
        .WithTags(Tags.Auth);
    }
}