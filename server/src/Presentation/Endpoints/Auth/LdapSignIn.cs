using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.LdapSignIn;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class LdapSignIn : IEndpoint
{
    public sealed record LdapSignInRequest(string UserName, string Password, string? RememberDeviceToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/ldap/sign-in", async (
            LdapSignInRequest request,
            bool? useCookies,
            bool? useSessionCookies,
            ICommandHandler<LdapSignInCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LdapSignInCommand(request.UserName, request.Password, request.RememberDeviceToken, useCookies, useSessionCookies);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Empty, Results.Problem);
        })
        .WithTags(EndpointTags.Auth);
    }
}
