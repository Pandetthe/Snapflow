using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Identity;

internal sealed class AppSignInManager(
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor) : ISignInManager
{
    private static AppUser EnsureIsAppUser(IUser user)
    {
        if (user is not AppUser appUser)
            throw new ArgumentException("User must be of type AppUser.", nameof(user));
        return appUser;
    }

    private static Result MapSignInResult(SignInResult result)
    {
        if (result.Succeeded)
            return Result.Success();
        else if (result.IsNotAllowed)
            return Result.Failure(UserErrors.SignInNotAllowed);
        else if (result.IsLockedOut)
            return Result.Failure(UserErrors.SignInLockedOut);
        else if (result.RequiresTwoFactor)
            return Result.Failure(UserErrors.SignInTwoFactorRequired);
        else
            return Result.Failure(UserErrors.SignInFailed);
    }

    private void FixHttpResponseStatus(bool useCookieScheme)
    {
        if (httpContextAccessor.HttpContext is { } context
            && context.Response.StatusCode == StatusCodes.Status200OK
            && useCookieScheme)
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
    }

    public async Task<Result> PasswordSignInAsync(IUser user, string password, bool? useCookies, bool? useSessionCookies, bool lockoutOnFailure)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(EnsureIsAppUser(user), password, isPersistent, lockoutOnFailure);
        FixHttpResponseStatus(useCookieScheme);
        return MapSignInResult(result);
    }

    public async Task<Result> TwoFactorAuthenticatorSignInAsync(string code, bool? useCookies, bool? useSessionCookies)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent, isPersistent);
        FixHttpResponseStatus(useCookieScheme);
        return MapSignInResult(result);
    }

    public async Task<Result> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        return MapSignInResult(result);
    }

    public async Task RefreshSignInAsync(IUser user)
    {
        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(EnsureIsAppUser(user), false);
    }

    public Task SignOutAsync() =>
        signInManager.SignOutAsync();
}
