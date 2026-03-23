using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using System.Globalization;

namespace Snapflow.Infrastructure.Auth.Managers;

internal sealed class AppUserManager(UserManager<AppUser> userManager) : IUserManager
{
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
}
