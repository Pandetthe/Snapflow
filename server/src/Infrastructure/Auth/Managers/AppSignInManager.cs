using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Auth.External;
using Snapflow.Infrastructure.Auth.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace Snapflow.Infrastructure.Auth.Managers;

internal sealed class AppSignInManager(
    SignInManager<AppUser> signInManager,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    IHttpContextAccessor httpContextAccessor,
    TimeProvider timeProvider,
    ExternalProviderRegistry providerRegistry,
    IDataProtectionProvider dataProtectionProvider,
    IPasskeyHandler<AppUser> passkeyHandler,
    PasskeyStateProtector passkeyStateProtector) : ISignInManager
{
    private static readonly TimeSpan TwoFactorTokenLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RememberDeviceTokenLifetime = TimeSpan.FromDays(14);

    private readonly ITimeLimitedDataProtector _twoFactorProtector = dataProtectionProvider
        .CreateProtector("Snapflow.Auth.TwoFactorSignIn")
        .ToTimeLimitedDataProtector();

    private readonly ITimeLimitedDataProtector _rememberDeviceProtector = dataProtectionProvider
        .CreateProtector("Snapflow.Auth.RememberDevice")
        .ToTimeLimitedDataProtector();

    private sealed record PendingTwoFactor(string UserId, string SecurityStamp, string? LoginProvider);

    private sealed record RememberedDevice(string UserId, string SecurityStamp);

    private HttpContext Context => httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("Signing in requires an HTTP request.");

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

    private async Task<Result> MapSignInResultAsync(
        SignInResult result,
        bool useCookieScheme,
        bool isPersistent,
        string? loginProvider,
        string? rememberDeviceToken)
    {
        if (result.RequiresTwoFactor && await signInManager.GetTwoFactorAuthenticationUserAsync() is { } user)
        {
            if (await IsDeviceRememberedAsync(user, rememberDeviceToken))
                return await CompleteRememberedSignInAsync(user, loginProvider, useCookieScheme, isPersistent);

            string? twoFactorToken = useCookieScheme ? null : await CreateTwoFactorTokenAsync(user, loginProvider);
            return Result.Failure(new TwoFactorRequiredError(twoFactorToken, await HasPasskeysAsync(user)));
        }

        return MapSignInResult(result);
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

    public async Task<Result> PasswordSignInAsync(IUser user, string password, bool? useCookies, bool? useSessionCookies, bool lockoutOnFailure, string? rememberDeviceToken)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        SignInResult result = await signInManager.PasswordSignInAsync(EnsureIsAppUser(user), password, isPersistent, lockoutOnFailure);
        FixHttpResponseStatus(useCookieScheme);
        return await MapSignInResultAsync(result, useCookieScheme, isPersistent, loginProvider: null, rememberDeviceToken);
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

    public Task<ExternalSignInTicket?> GetExternalSignInAsync() => ReadExternalLoginAsync(expectedXsrf: null);

    public async Task<ExternalIdentity?> GetExternalLinkAsync(IUser user)
    {
        string userId = await signInManager.UserManager.GetUserIdAsync(EnsureIsAppUser(user));
        return (await ReadExternalLoginAsync(userId))?.Identity;
    }

    private async Task<ExternalSignInTicket?> ReadExternalLoginAsync(string? expectedXsrf)
    {
        ExternalLoginInfo? info = await signInManager.GetExternalLoginInfoAsync(expectedXsrf);
        if (info is null)
            return null;

        ExternalProvider? provider = providerRegistry.FindRedirectProvider(info.LoginProvider);
        if (provider is null)
            return null;

        string? email = FindFirstValue(info.Principal, provider.EmailClaimTypes);
        bool emailVerified = email is not null && (provider.TrustEmail
            || string.Equals(info.Principal.FindFirstValue(ExternalProviderRegistry.EmailVerifiedClaimType), "true", StringComparison.OrdinalIgnoreCase));
        bool isPersistent = info.AuthenticationProperties?.Items.TryGetValue(ExternalProviderRegistry.PersistentItemKey, out string? persistent) == true
            && persistent == "true";

        var identity = new ExternalIdentity(
            info.LoginProvider,
            info.ProviderKey,
            provider.DisplayName,
            email,
            emailVerified,
            FindFirstValue(info.Principal, provider.NameClaimTypes));

        return new ExternalSignInTicket(identity, isPersistent);
    }

    public async Task<Result> ExternalLoginSignInAsync(ExternalIdentity identity, bool? useCookies, bool? useSessionCookies, string? rememberDeviceToken)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        SignInResult result = await signInManager.ExternalLoginSignInAsync(identity.Provider, identity.ProviderKey, isPersistent, bypassTwoFactor: false);
        FixHttpResponseStatus(useCookieScheme);
        return await MapSignInResultAsync(result, useCookieScheme, isPersistent, identity.Provider, rememberDeviceToken);
    }

    public async Task SignOutExternalAsync()
    {
        if (httpContextAccessor.HttpContext is { } context)
        {
            await context.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    public async Task<Result> TwoFactorSignInAsync(
        string? code,
        string? recoveryCode,
        string? passkeyCredential,
        string? passkeyState,
        bool rememberDevice,
        string? twoFactorToken,
        bool? useCookies,
        bool? useSessionCookies)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);

        (AppUser? user, string? loginProvider) = await FindPendingTwoFactorAsync(twoFactorToken);
        if (user is null)
            return Result.Failure(TwoFactorErrors.SignInExpired);

        if (!string.IsNullOrWhiteSpace(passkeyCredential))
            return await VerifyPasskeyAndSignInAsync(user, loginProvider, passkeyCredential, passkeyState ?? string.Empty, rememberDevice, useCookieScheme, isPersistent);

        if (!string.IsNullOrEmpty(twoFactorToken) || !useCookieScheme)
            return await VerifyCodeAndSignInAsync(user, loginProvider, code, recoveryCode, rememberDevice, useCookieScheme, isPersistent);

        signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

        SignInResult result = string.IsNullOrWhiteSpace(recoveryCode)
            ? await signInManager.TwoFactorAuthenticatorSignInAsync(
                TwoFactorCode.NormalizeAuthenticatorCode(code ?? string.Empty), isPersistent, rememberDevice)
            : await signInManager.TwoFactorRecoveryCodeSignInAsync(TwoFactorCode.NormalizeRecoveryCode(recoveryCode));

        FixHttpResponseStatus(useCookieScheme);

        return result.Succeeded || result.IsLockedOut || result.IsNotAllowed
            ? MapSignInResult(result)
            : Result.Failure(TwoFactorErrors.InvalidCode);
    }

    public async Task<Result<PasskeyChallenge>> CreateTwoFactorPasskeyOptionsAsync(string? twoFactorToken)
    {
        (AppUser? user, _) = await FindPendingTwoFactorAsync(twoFactorToken);
        if (user is null)
            return Result.Failure<PasskeyChallenge>(TwoFactorErrors.SignInExpired);

        if (!await HasPasskeysAsync(user))
            return Result.Failure<PasskeyChallenge>(PasskeyErrors.NoneRegistered);

        PasskeyRequestOptionsResult options = await passkeyHandler.MakeRequestOptionsAsync(user, Context);
        string userId = await signInManager.UserManager.GetUserIdAsync(user);

        return new PasskeyChallenge(
            options.RequestOptionsJson,
            passkeyStateProtector.Protect(PasskeyStateProtector.TwoFactor, userId, options.AssertionState));
    }

    public async Task<PasskeyChallenge> CreatePasskeySignInOptionsAsync()
    {
        PasskeyRequestOptionsResult options = await passkeyHandler.MakeRequestOptionsAsync(null, Context);

        return new PasskeyChallenge(
            options.RequestOptionsJson,
            passkeyStateProtector.Protect(PasskeyStateProtector.SignIn, null, options.AssertionState));
    }

    public async Task<Result> PasskeySignInAsync(string credentialJson, string state, bool? useCookies, bool? useSessionCookies)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        UserManager<AppUser> userManager = signInManager.UserManager;

        PasskeyCeremony? ceremony = await passkeyStateProtector.ConsumeAsync(state, PasskeyStateProtector.SignIn);
        if (ceremony is null)
            return Result.Failure(PasskeyErrors.Expired);

        PasskeyAssertionResult<AppUser> assertion = await passkeyHandler.PerformAssertionAsync(new PasskeyAssertionContext
        {
            HttpContext = Context,
            CredentialJson = credentialJson,
            AssertionState = ceremony.State
        });

        if (!assertion.Succeeded)
            return Result.Failure(PasskeyErrors.NotRecognized);

        AppUser user = assertion.User;
        if (user.IsDeleted)
            return Result.Failure(UserErrors.AccountDeleted);
        if (!await signInManager.CanSignInAsync(user))
            return Result.Failure(UserErrors.SignInNotAllowed);
        if (await userManager.IsLockedOutAsync(user))
            return Result.Failure(UserErrors.SignInLockedOut);

        await userManager.AddOrUpdatePasskeyAsync(user, assertion.Passkey);
        await SignOutPendingTwoFactorAsync();

        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        await signInManager.SignInWithClaimsAsync(user, isPersistent, [new Claim("amr", "mfa")]);
        FixHttpResponseStatus(useCookieScheme);

        return Result.Success();
    }

    public async Task RefreshSignInAsync(IUser user)
    {
        if (httpContextAccessor.HttpContext is not { } context)
            return;

        AuthenticateResult authentication = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authentication.Succeeded)
            return;

        signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;
        await signInManager.RefreshSignInAsync(EnsureIsAppUser(user));
    }

    private async Task<(AppUser? User, string? LoginProvider)> FindPendingTwoFactorAsync(string? twoFactorToken)
    {
        if (!string.IsNullOrEmpty(twoFactorToken))
        {
            UserManager<AppUser> userManager = signInManager.UserManager;
            PendingTwoFactor? pending = Unprotect<PendingTwoFactor>(_twoFactorProtector, twoFactorToken);
            AppUser? tokenUser = pending is null ? null : await userManager.FindByIdAsync(pending.UserId);

            if (pending is null || tokenUser is null || tokenUser.IsDeleted
                || !string.Equals(await userManager.GetSecurityStampAsync(tokenUser) ?? string.Empty, pending.SecurityStamp, StringComparison.Ordinal))
            {
                return (null, null);
            }

            return (tokenUser, pending.LoginProvider);
        }

        AppUser? cookieUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (cookieUser is null)
            return (null, null);

        AuthenticateResult pendingCookie = await Context.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
        return (cookieUser, pendingCookie.Principal?.FindFirstValue(ClaimTypes.AuthenticationMethod));
    }

    private async Task<Result> VerifyCodeAndSignInAsync(
        AppUser user,
        string? loginProvider,
        string? code,
        string? recoveryCode,
        bool rememberDevice,
        bool useCookieScheme,
        bool isPersistent)
    {
        UserManager<AppUser> userManager = signInManager.UserManager;

        if (await userManager.IsLockedOutAsync(user))
            return Result.Failure(UserErrors.SignInLockedOut);

        bool usesRecoveryCode = !string.IsNullOrWhiteSpace(recoveryCode);
        if (usesRecoveryCode)
        {
            IdentityResult redeemed = await userManager.RedeemTwoFactorRecoveryCodeAsync(
                user,
                TwoFactorCode.NormalizeRecoveryCode(recoveryCode!));

            if (!redeemed.Succeeded)
            {
                await userManager.AccessFailedAsync(user);
                return Result.Failure(await userManager.IsLockedOutAsync(user)
                    ? UserErrors.SignInLockedOut
                    : TwoFactorErrors.InvalidCode);
            }
        }
        else
        {
            bool verified = await userManager.VerifyTwoFactorTokenAsync(
                user,
                userManager.Options.Tokens.AuthenticatorTokenProvider,
                TwoFactorCode.NormalizeAuthenticatorCode(code ?? string.Empty));

            if (!verified)
            {
                await userManager.AccessFailedAsync(user);
                return Result.Failure(await userManager.IsLockedOutAsync(user)
                    ? UserErrors.SignInLockedOut
                    : TwoFactorErrors.InvalidCode);
            }
        }

        return await CompleteTwoFactorSignInAsync(user, loginProvider, rememberDevice && !usesRecoveryCode, useCookieScheme, isPersistent);
    }

    private async Task<Result> VerifyPasskeyAndSignInAsync(
        AppUser user,
        string? loginProvider,
        string credentialJson,
        string state,
        bool rememberDevice,
        bool useCookieScheme,
        bool isPersistent)
    {
        UserManager<AppUser> userManager = signInManager.UserManager;

        if (!providerRegistry.PasswordAuthenticationEnabled)
            return Result.Failure(AuthenticationErrors.PasswordAuthenticationDisabled);

        if (await userManager.IsLockedOutAsync(user))
            return Result.Failure(UserErrors.SignInLockedOut);

        string userId = await userManager.GetUserIdAsync(user);
        PasskeyCeremony? ceremony = await passkeyStateProtector.ConsumeAsync(state, PasskeyStateProtector.TwoFactor);
        if (ceremony is null || !string.Equals(ceremony.UserId, userId, StringComparison.Ordinal))
            return Result.Failure(PasskeyErrors.Expired);

        PasskeyAssertionResult<AppUser> assertion = await passkeyHandler.PerformAssertionAsync(new PasskeyAssertionContext
        {
            HttpContext = Context,
            CredentialJson = credentialJson,
            AssertionState = ceremony.State
        });

        if (!assertion.Succeeded || assertion.User.Id != user.Id)
            return Result.Failure(PasskeyErrors.NotRecognized);

        await userManager.AddOrUpdatePasskeyAsync(user, assertion.Passkey);

        return await CompleteTwoFactorSignInAsync(user, loginProvider, rememberDevice, useCookieScheme, isPersistent);
    }

    private async Task<Result> CompleteTwoFactorSignInAsync(
        AppUser user,
        string? loginProvider,
        bool rememberDevice,
        bool useCookieScheme,
        bool isPersistent)
    {
        await signInManager.UserManager.ResetAccessFailedCountAsync(user);
        await SignOutPendingTwoFactorAsync();

        if (rememberDevice)
        {
            if (useCookieScheme)
                await signInManager.RememberTwoFactorClientAsync(user);
            else if (httpContextAccessor.HttpContext is { } context)
                context.Response.Headers[AuthHeaderNames.RememberDeviceToken] = await CreateRememberDeviceTokenAsync(user);
        }

        List<Claim> claims = [new("amr", "mfa")];
        if (loginProvider is not null)
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, loginProvider));

        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        await signInManager.SignInWithClaimsAsync(user, isPersistent, claims);
        FixHttpResponseStatus(useCookieScheme);

        return Result.Success();
    }

    private async Task<Result> CompleteRememberedSignInAsync(AppUser user, string? loginProvider, bool useCookieScheme, bool isPersistent)
    {
        await signInManager.UserManager.ResetAccessFailedCountAsync(user);
        await SignOutPendingTwoFactorAsync();

        Claim[] claims = loginProvider is null
            ? [new Claim("amr", "pwd")]
            : [new Claim(ClaimTypes.AuthenticationMethod, loginProvider)];

        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        await signInManager.SignInWithClaimsAsync(user, isPersistent, claims);
        FixHttpResponseStatus(useCookieScheme);

        return Result.Success();
    }

    private async Task SignOutPendingTwoFactorAsync()
    {
        if (httpContextAccessor.HttpContext is { } context)
        {
            await context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
        }
    }

    private async Task<bool> HasPasskeysAsync(AppUser user) =>
        providerRegistry.PasswordAuthenticationEnabled
        && (await signInManager.UserManager.GetPasskeysAsync(user)).Count > 0;

    private async Task<bool> IsDeviceRememberedAsync(AppUser user, string? rememberDeviceToken)
    {
        if (string.IsNullOrEmpty(rememberDeviceToken)
            || Unprotect<RememberedDevice>(_rememberDeviceProtector, rememberDeviceToken) is not { } device)
        {
            return false;
        }

        UserManager<AppUser> userManager = signInManager.UserManager;
        return string.Equals(device.UserId, await userManager.GetUserIdAsync(user), StringComparison.Ordinal)
            && string.Equals(device.SecurityStamp, await userManager.GetSecurityStampAsync(user) ?? string.Empty, StringComparison.Ordinal);
    }

    private async Task<string> CreateTwoFactorTokenAsync(AppUser user, string? loginProvider)
    {
        UserManager<AppUser> userManager = signInManager.UserManager;
        var pending = new PendingTwoFactor(
            await userManager.GetUserIdAsync(user),
            await userManager.GetSecurityStampAsync(user) ?? string.Empty,
            loginProvider);

        return _twoFactorProtector.Protect(JsonSerializer.Serialize(pending), TwoFactorTokenLifetime);
    }

    private async Task<string> CreateRememberDeviceTokenAsync(AppUser user)
    {
        UserManager<AppUser> userManager = signInManager.UserManager;
        var device = new RememberedDevice(
            await userManager.GetUserIdAsync(user),
            await userManager.GetSecurityStampAsync(user) ?? string.Empty);

        return _rememberDeviceProtector.Protect(JsonSerializer.Serialize(device), RememberDeviceTokenLifetime);
    }

    private static T? Unprotect<T>(ITimeLimitedDataProtector protector, string value) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(protector.Unprotect(value));
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            return null;
        }
    }

    private static string? FindFirstValue(ClaimsPrincipal principal, IEnumerable<string> claimTypes) =>
        claimTypes
            .Select(principal.FindFirstValue)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
