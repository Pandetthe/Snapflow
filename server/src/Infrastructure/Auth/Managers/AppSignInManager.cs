using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;

namespace Snapflow.Infrastructure.Auth.Managers;

internal sealed class AppSignInManager(
    SignInManager<AppUser> signInManager,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    IHttpContextAccessor httpContextAccessor,
    TimeProvider timeProvider) : ISignInManager
{
    private static AppUser EnsureIsAppUser(IUser user)
    {
        return user as AppUser ?? throw new ArgumentException("User must be of type AppUser.", nameof(user));
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
        if (httpContextAccessor.HttpContext is { Response.StatusCode: StatusCodes.Status200OK } context
            && useCookieScheme)
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
    }
    
    private async Task SignOutFromAllConfiguredSchemesAsync()
    {
        if (httpContextAccessor.HttpContext is { } context)
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        }

        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await signInManager.SignOutAsync();
    }

    public async Task<Result> PasswordSignInAsync(IUser user, string password, bool? useCookies, bool? useSessionCookies, bool lockoutOnFailure)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        SignInResult result = await signInManager.PasswordSignInAsync(EnsureIsAppUser(user), password, isPersistent, lockoutOnFailure);
        FixHttpResponseStatus(useCookieScheme);
        return MapSignInResult(result);
    }

    public async Task<Result> TwoFactorAuthenticatorSignInAsync(string code, bool? useCookies, bool? useSessionCookies)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        SignInResult result = await signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent, isPersistent);
        FixHttpResponseStatus(useCookieScheme);
        return MapSignInResult(result);
    }

    public async Task<Result> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        SignInResult result = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        return MapSignInResult(result);
    }

    public async Task<Result> RefreshSignInAsync(string refreshToken)
    {
        var protector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        AuthenticationTicket? ticket = protector.Unprotect(refreshToken);

        if (ticket?.Properties.ExpiresUtc == null || timeProvider.GetUtcNow() >= ticket.Properties.ExpiresUtc)
        {
            return Result.Failure(UserErrors.RefreshFailed);
        }

        AppUser? user = await signInManager.ValidateSecurityStampAsync(ticket.Principal);
        if (user == null)
        {
            return Result.Failure(UserErrors.RefreshFailed);
        }

        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(user, false);

        return Result.Success();
    }

    public async Task<Result> SignOutAllAsync(IUser user)
    {
        IdentityResult result = await signInManager.UserManager.UpdateSecurityStampAsync(EnsureIsAppUser(user));
        if (!result.Succeeded)
        {
            return Result.Failure(UserErrors.SignOutFailed);
        }

        await SignOutFromAllConfiguredSchemesAsync();

        return Result.Success();
    }

    public Task SignOutAsync() => SignOutFromAllConfiguredSchemesAsync();
}
