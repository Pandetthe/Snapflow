using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.External;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.LdapSignIn;

internal sealed class LdapSignInHandler(
    ILdapAuthenticator ldapAuthenticator,
    ExternalAccountSignIn accountSignIn)
    : ICommandHandler<LdapSignInCommand>
{
    public async Task<Result> Handle(LdapSignInCommand command, CancellationToken cancellationToken = default)
    {
        if (!ldapAuthenticator.IsEnabled)
            return Result.Failure(AuthenticationErrors.ProviderNotAvailable);

        Result<ExternalIdentity> identity = await ldapAuthenticator.AuthenticateAsync(command.UserName, command.Password, cancellationToken);
        if (identity.IsFailure)
            return identity;

        return await accountSignIn.SignInAsync(identity.Value, command.UseCookies, command.UseSessionCookies, command.RememberDeviceToken);
    }
}
