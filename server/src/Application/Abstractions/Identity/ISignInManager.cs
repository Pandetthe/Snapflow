using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface ISignInManager
{
    Task<Result> PasswordSignInAsync(IUser user, string password, bool? useCookies, bool? useSessionCookies, bool lockoutOnFailure);

    Task<Result> TwoFactorAuthenticatorSignInAsync(string code, bool? useCookies, bool? useSessionCookies);

    Task<Result> TwoFactorRecoveryCodeSignInAsync(string recoveryCode);

    Task<Result> RefreshSignInAsync(string refreshToken);

    Task<Result> SignOutAllAsync(IUser user);

    Task SignOutAsync();
}
