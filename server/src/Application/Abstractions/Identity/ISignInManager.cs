using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface ISignInManager
{
    Task<Result> PasswordSignInAsync(IUser user, string password, bool? useCookies, bool? useSessionCookies, bool lockoutOnFailure, string? rememberDeviceToken);

    Task<Result> RefreshSignInAsync(string refreshToken);

    Task<Result> SignOutAllAsync(IUser user);

    Task SignOutAsync();

    Task<ExternalSignInTicket?> GetExternalSignInAsync();

    Task<ExternalIdentity?> GetExternalLinkAsync(IUser user);

    Task<Result> ExternalLoginSignInAsync(ExternalIdentity identity, bool? useCookies, bool? useSessionCookies, string? rememberDeviceToken);

    Task SignOutExternalAsync();

    Task<Result> TwoFactorSignInAsync(
        string? code,
        string? recoveryCode,
        string? passkeyCredential,
        string? passkeyState,
        bool rememberDevice,
        string? twoFactorToken,
        bool? useCookies,
        bool? useSessionCookies);

    Task<Result<PasskeyChallenge>> CreateTwoFactorPasskeyOptionsAsync(string? twoFactorToken);

    Task<PasskeyChallenge> CreatePasskeySignInOptionsAsync();

    Task<Result> PasskeySignInAsync(string credentialJson, string state, bool? useCookies, bool? useSessionCookies);

    Task RefreshSignInAsync(IUser user);
}
