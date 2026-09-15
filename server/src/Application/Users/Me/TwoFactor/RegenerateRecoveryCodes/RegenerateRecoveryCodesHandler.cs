using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.RegenerateRecoveryCodes;

internal sealed class RegenerateRecoveryCodesHandler(
    IUserContext userContext,
    IUserManager userManager) : ICommandHandler<RegenerateRecoveryCodesCommand, RegenerateRecoveryCodesResponse>
{
    public async Task<Result<RegenerateRecoveryCodesResponse>> Handle(RegenerateRecoveryCodesCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        TwoFactorStatus status = await userManager.GetTwoFactorStatusAsync(user);
        if (!status.IsEnabled)
            return Result.Failure<RegenerateRecoveryCodesResponse>(TwoFactorErrors.NotEnabled);

        if (!await userManager.VerifyAuthenticatorCodeAsync(user, command.Code))
            return Result.Failure<RegenerateRecoveryCodesResponse>(TwoFactorErrors.InvalidCode);

        IReadOnlyList<string> codes = await userManager.GenerateRecoveryCodesAsync(user);

        return new RegenerateRecoveryCodesResponse(codes);
    }
}
