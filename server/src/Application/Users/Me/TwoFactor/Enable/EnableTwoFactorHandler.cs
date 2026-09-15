using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.Enable;

internal sealed class EnableTwoFactorHandler(
    IUserContext userContext,
    IUserManager userManager,
    ISignInManager signInManager) : ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    public async Task<Result<EnableTwoFactorResponse>> Handle(EnableTwoFactorCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        TwoFactorStatus status = await userManager.GetTwoFactorStatusAsync(user);
        if (status.IsEnabled)
            return Result.Failure<EnableTwoFactorResponse>(TwoFactorErrors.AlreadyEnabled);

        if (!await userManager.VerifyAuthenticatorCodeAsync(user, command.Code))
            return Result.Failure<EnableTwoFactorResponse>(TwoFactorErrors.InvalidCode);

        Result<IReadOnlyList<string>> enabled = await userManager.EnableTwoFactorAsync(user);
        if (enabled.IsFailure)
            return Result.Failure<EnableTwoFactorResponse>(enabled.Error);

        await signInManager.RefreshSignInAsync(user);

        return new EnableTwoFactorResponse(enabled.Value);
    }
}
