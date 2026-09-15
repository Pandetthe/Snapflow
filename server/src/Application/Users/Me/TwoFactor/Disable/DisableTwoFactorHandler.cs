using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.Disable;

internal sealed class DisableTwoFactorHandler(
    IUserContext userContext,
    IUserManager userManager,
    ISignInManager signInManager) : ICommandHandler<DisableTwoFactorCommand>
{
    public async Task<Result> Handle(DisableTwoFactorCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        TwoFactorStatus status = await userManager.GetTwoFactorStatusAsync(user);
        if (!status.IsEnabled)
            return Result.Failure(TwoFactorErrors.NotEnabled);

        bool verified = string.IsNullOrWhiteSpace(command.RecoveryCode)
            ? await userManager.VerifyAuthenticatorCodeAsync(user, command.Code ?? string.Empty)
            : await userManager.RedeemRecoveryCodeAsync(user, command.RecoveryCode);

        if (!verified)
            return Result.Failure(TwoFactorErrors.InvalidCode);

        Result disabled = await userManager.DisableTwoFactorAsync(user);
        if (disabled.IsFailure)
            return disabled;

        await signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }
}
