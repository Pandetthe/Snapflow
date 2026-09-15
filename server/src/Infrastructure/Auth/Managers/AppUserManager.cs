using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Roles;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using System.Globalization;

namespace Snapflow.Infrastructure.Auth.Managers;

internal sealed class AppUserManager(UserManager<AppUser> userManager) : IUserManager
{
    private const string AuthenticatorIssuer = "Snapflow";

    private static AppUser EnsureIsAppUser(IUser user)
    {
        return user as AppUser ?? throw new ArgumentException("User must be of type AppUser.", nameof(user));
    }

    public async Task<IUser?> FindByIdAsync(int userId) =>
        await userManager.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));

    public async Task<IUser?> FindByEmailAsync(string email) =>
        await userManager.FindByEmailAsync(email);

    public async Task<IUser?> FindByNameAsync(string userName) =>
        await userManager.FindByNameAsync(userName);

    private static string? GetPropertyName(string code)
    {
        return code switch
        {
            "DuplicateUserName" or "InvalidUserName" => nameof(IUser.UserName),
            "DuplicateEmail" or "InvalidEmail" => nameof(IUser.Email),
            _ => null,
        };
    }

    public async Task<Result<IUser>> CreateAsync(string email, string userName, string password)
    {
        var user = AppUser.Create(email, userName);

        IdentityResult result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            return Result.Success<IUser>(user);

        var errors = result.Errors.Select(e => new PropertyValidationError(GetPropertyName(e.Code), e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(IUser user) =>
        await userManager.GenerateEmailConfirmationTokenAsync(EnsureIsAppUser(user));

    public async Task<string> GenerateEmailChangeTokenAsync(IUser user, string newEmail) =>
        await userManager.GenerateChangeEmailTokenAsync(EnsureIsAppUser(user), newEmail);

    public async Task<Result> ConfirmEmailAsync(IUser user, string token)
    {
        IdentityResult result = await userManager.ConfirmEmailAsync(EnsureIsAppUser(user), token);
        if (result.Succeeded)
            return Result.Success();

        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<Result> ChangeEmailAsync(IUser user, string newEmail, string token)
    {
        IdentityResult result = await userManager.ChangeEmailAsync(EnsureIsAppUser(user), newEmail, token);
        if (result.Succeeded)
            return Result.Success();

        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public Task<bool> IsEmailConfirmedAsync(IUser user) =>
        userManager.IsEmailConfirmedAsync(EnsureIsAppUser(user));

    public Task<string> GeneratePasswordResetTokenAsync(IUser user) =>
        userManager.GeneratePasswordResetTokenAsync(EnsureIsAppUser(user));

    public async Task<Result> ResetPasswordAsync(IUser user, string code, string newPassword)
    {
        IdentityResult result = await userManager.ResetPasswordAsync(EnsureIsAppUser(user), code, newPassword);
        if (result.Succeeded)
            return Result.Success();

        if (result.Errors.Any(e => e.Code == "InvalidToken"))
            return Result.Failure(UserErrors.PasswordResetInvalidCode);
        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<Result> UpdateUserNameAsync(IUser user, string userName)
    {
        AppUser appUser = EnsureIsAppUser(user);
        appUser.UserName = userName;
        appUser.Raise(u => new UserProfileUpdatedDomainEvent(u.Id));

        IdentityResult result = await userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            return Result.Success();

        appUser.ClearDomainEvents();
        var errors = result.Errors.Select(e => new PropertyValidationError(GetPropertyName(e.Code), e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<Result> ChangePasswordAsync(IUser user, string currentPassword, string newPassword)
    {
        AppUser appUser = EnsureIsAppUser(user);
        IdentityResult result = await userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        if (result.Succeeded)
            return Result.Success();

        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<Result> UpdateAvatarAsync(IUser user, AvatarType avatarType, byte[]? avatarData, string? contentType)
    {
        AppUser appUser = EnsureIsAppUser(user);
        appUser.AvatarType = avatarType;
        appUser.AvatarData = avatarData;
        appUser.AvatarContentType = contentType;
        appUser.Raise(u => new UserProfileUpdatedDomainEvent(u.Id));
        IdentityResult result = await userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            return Result.Success();

        appUser.ClearDomainEvents();
        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<Result> SoftDeleteAsync(IUser user, TimeProvider timeProvider)
    {
        AppUser appUser = EnsureIsAppUser(user);
        appUser.IsDeleted = true;
        appUser.DeletedAt = timeProvider.GetUtcNow();
        appUser.Raise(u => new UserDeletedDomainEvent(u.Id));

        IdentityResult stampResult = await userManager.UpdateSecurityStampAsync(appUser);
        if (!stampResult.Succeeded)
        {
            appUser.ClearDomainEvents();
            var stampErrors = stampResult.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
            return Result.ValidationFailure<IUser>(new ValidationError(stampErrors));
        }

        IdentityResult result = await userManager.UpdateAsync(appUser);
        if (result.Succeeded)
            return Result.Success();

        appUser.ClearDomainEvents();
        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(IUser user) =>
        [.. await userManager.GetRolesAsync(EnsureIsAppUser(user))];

    public async Task<Result> AddToRoleAsync(IUser user, string role)
    {
        AppUser appUser = EnsureIsAppUser(user);
        if (!SystemRoles.All.Contains(role))
            return Result.Failure(RoleErrors.NotFound(role));
        if (await userManager.IsInRoleAsync(appUser, role))
            return Result.Failure(RoleErrors.AlreadyAssigned(role));

        IdentityResult result = await userManager.AddToRoleAsync(appUser, role);
        if (!result.Succeeded)
            return IdentityFailure(result);

        // Role claims baked into an existing cookie or token go stale; permission checks read the database,
        // but refreshing the stamp keeps role claims honest for anything relying on them.
        return await RefreshSecurityStampAsync(appUser);
    }

    public async Task<Result> RemoveFromRoleAsync(IUser user, string role)
    {
        AppUser appUser = EnsureIsAppUser(user);
        if (!SystemRoles.All.Contains(role))
            return Result.Failure(RoleErrors.NotFound(role));
        if (!await userManager.IsInRoleAsync(appUser, role))
            return Result.Failure(RoleErrors.NotAssigned(role));

        IdentityResult result = await userManager.RemoveFromRoleAsync(appUser, role);
        if (!result.Succeeded)
            return IdentityFailure(result);

        return await RefreshSecurityStampAsync(appUser);
    }

    public async Task<IUser?> FindByLoginAsync(string provider, string providerKey) =>
        await userManager.FindByLoginAsync(provider, providerKey);

    public async Task<Result> AddLoginAsync(IUser user, ExternalIdentity identity)
    {
        IdentityResult result = await userManager.AddLoginAsync(EnsureIsAppUser(user), ToLoginInfo(identity));
        return result.Succeeded ? Result.Success() : IdentityFailure(result);
    }

    public async Task<IReadOnlyList<ExternalLoginDetails>> GetLoginsAsync(IUser user) =>
        [.. (await userManager.GetLoginsAsync(EnsureIsAppUser(user)))
            .Select(login => new ExternalLoginDetails(login.LoginProvider, login.ProviderDisplayName ?? login.LoginProvider))];

    public async Task<Result> RemoveLoginAsync(IUser user, string provider)
    {
        AppUser appUser = EnsureIsAppUser(user);
        UserLoginInfo? login = (await userManager.GetLoginsAsync(appUser))
            .FirstOrDefault(l => string.Equals(l.LoginProvider, provider, StringComparison.Ordinal));
        if (login is null)
            return Result.Failure(ExternalLoginErrors.NotFound);

        IdentityResult result = await userManager.RemoveLoginAsync(appUser, login.LoginProvider, login.ProviderKey);
        return result.Succeeded ? Result.Success() : IdentityFailure(result);
    }

    public Task<bool> HasPasswordAsync(IUser user) =>
        userManager.HasPasswordAsync(EnsureIsAppUser(user));

    public async Task<Result> AddPasswordAsync(IUser user, string password)
    {
        IdentityResult result = await userManager.AddPasswordAsync(EnsureIsAppUser(user), password);
        return result.Succeeded ? Result.Success() : IdentityFailure(result);
    }

    public async Task<Result<IUser>> CreateExternalAsync(ExternalIdentity identity, string userName)
    {
        if (string.IsNullOrWhiteSpace(identity.Email))
            throw new ArgumentException("An external account needs an email.", nameof(identity));

        var user = AppUser.Create(identity.Email, userName);
        user.EmailConfirmed = identity.EmailVerified;

        IdentityResult created = await userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            var errors = created.Errors.Select(e => new PropertyValidationError(GetPropertyName(e.Code), e.Code, e.Description)).ToArray();
            return Result.ValidationFailure<IUser>(new ValidationError(errors));
        }

        IdentityResult linked = await userManager.AddLoginAsync(user, ToLoginInfo(identity));
        if (linked.Succeeded)
            return Result.Success<IUser>(user);

        await userManager.DeleteAsync(user);
        return IdentityFailure(linked);
    }

    public async Task<TwoFactorStatus> GetTwoFactorStatusAsync(IUser user)
    {
        AppUser appUser = EnsureIsAppUser(user);
        return new TwoFactorStatus(
            await userManager.GetTwoFactorEnabledAsync(appUser),
            await userManager.CountRecoveryCodesAsync(appUser));
    }

    public async Task<AuthenticatorSetup> GetAuthenticatorSetupAsync(IUser user)
    {
        AppUser appUser = EnsureIsAppUser(user);

        string? key = await userManager.GetAuthenticatorKeyAsync(appUser);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(appUser);
            key = await userManager.GetAuthenticatorKeyAsync(appUser)
                  ?? throw new InvalidOperationException("The authenticator key could not be created.");
        }

        string issuer = Uri.EscapeDataString(AuthenticatorIssuer);
        string account = Uri.EscapeDataString(appUser.Email ?? appUser.UserName ?? appUser.Id.ToString(CultureInfo.InvariantCulture));
        string uri = $"otpauth://totp/{issuer}:{account}?secret={key}&issuer={issuer}&digits=6";

        return new AuthenticatorSetup(key, uri);
    }

    public Task<bool> VerifyAuthenticatorCodeAsync(IUser user, string code) =>
        userManager.VerifyTwoFactorTokenAsync(
            EnsureIsAppUser(user),
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            TwoFactorCode.NormalizeAuthenticatorCode(code));

    public async Task<bool> RedeemRecoveryCodeAsync(IUser user, string recoveryCode)
    {
        IdentityResult result = await userManager.RedeemTwoFactorRecoveryCodeAsync(
            EnsureIsAppUser(user),
            TwoFactorCode.NormalizeRecoveryCode(recoveryCode));
        return result.Succeeded;
    }

    public async Task<Result<IReadOnlyList<string>>> EnableTwoFactorAsync(IUser user)
    {
        AppUser appUser = EnsureIsAppUser(user);

        IdentityResult result = await userManager.SetTwoFactorEnabledAsync(appUser, true);
        if (!result.Succeeded)
            return Result.ValidationFailure<IReadOnlyList<string>>(ToValidationError(result));

        return Result.Success<IReadOnlyList<string>>(await GenerateRecoveryCodesAsync(appUser));
    }

    public async Task<Result> DisableTwoFactorAsync(IUser user)
    {
        AppUser appUser = EnsureIsAppUser(user);

        IdentityResult disabled = await userManager.SetTwoFactorEnabledAsync(appUser, false);
        if (!disabled.Succeeded)
            return IdentityFailure(disabled);

        IdentityResult reset = await userManager.ResetAuthenticatorKeyAsync(appUser);
        return reset.Succeeded ? Result.Success() : IdentityFailure(reset);
    }

    public async Task<IReadOnlyList<string>> GenerateRecoveryCodesAsync(IUser user)
    {
        IEnumerable<string>? codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(
            EnsureIsAppUser(user),
            Domain.Users.UserOptions.TwoFactorRecoveryCodeCount);
        return [.. codes ?? []];
    }

    private static ValidationError ToValidationError(IdentityResult result) =>
        new(result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray());

    private static UserLoginInfo ToLoginInfo(ExternalIdentity identity) =>
        new(identity.Provider, identity.ProviderKey, identity.ProviderDisplayName);

    private async Task<Result> RefreshSecurityStampAsync(AppUser appUser)
    {
        IdentityResult result = await userManager.UpdateSecurityStampAsync(appUser);
        return result.Succeeded ? Result.Success() : IdentityFailure(result);
    }

    private static Result<IUser> IdentityFailure(IdentityResult result)
    {
        var errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
        return Result.ValidationFailure<IUser>(new ValidationError(errors));
    }
}
