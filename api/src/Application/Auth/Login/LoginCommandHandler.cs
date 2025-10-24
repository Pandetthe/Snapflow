using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.Login;

internal sealed class LoginCommandHandler(
    ISignInManager signInManager,
    IUserManager userManager)
    : ICommandHandler<LoginCommand>
{
    public async Task<Result> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result.Failure(UserErrors.SignInFailed);
        var result = await signInManager.PasswordSignInAsync(user, command.Password, command.UseCookies, command.UseSessionCookies, true);

        if (!result.IsSuccess && result.Error.Code != UserErrors.SignInTwoFactorRequired.Code)
            return result;
        if (!string.IsNullOrEmpty(command.TwoFactorCode))
        {
            result = await signInManager.TwoFactorAuthenticatorSignInAsync(command.TwoFactorCode, command.UseCookies, command.UseSessionCookies);
        }
        else if (!string.IsNullOrEmpty(command.TwoFactorRecoveryCode))
        {
            result = await signInManager.TwoFactorRecoveryCodeSignInAsync(command.TwoFactorRecoveryCode);
        }
        return result;
    }
}