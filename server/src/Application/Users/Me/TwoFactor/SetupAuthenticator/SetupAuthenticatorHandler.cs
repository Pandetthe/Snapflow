using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.SetupAuthenticator;

internal sealed class SetupAuthenticatorHandler(
    IUserContext userContext,
    IUserManager userManager,
    ISignInManager signInManager) : ICommandHandler<SetupAuthenticatorCommand, SetupAuthenticatorResponse>
{
    public async Task<Result<SetupAuthenticatorResponse>> Handle(SetupAuthenticatorCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        TwoFactorStatus status = await userManager.GetTwoFactorStatusAsync(user);
        if (status.IsEnabled)
            return Result.Failure<SetupAuthenticatorResponse>(TwoFactorErrors.AlreadyEnabled);

        AuthenticatorSetup setup = await userManager.GetAuthenticatorSetupAsync(user);
        await signInManager.RefreshSignInAsync(user);

        return new SetupAuthenticatorResponse(setup.SharedKey, setup.AuthenticatorUri);
    }
}
