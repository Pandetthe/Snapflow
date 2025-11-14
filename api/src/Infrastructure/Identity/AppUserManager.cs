using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Identity.Entities;
using System.Globalization;
using System.Linq;

namespace Snapflow.Infrastructure.Identity;

internal sealed class AppUserManager(UserManager<AppUser> userManager) : IUserManager
{
    private static AppUser EnsureIsAppUser(IUser user)
    {
        if (user is not AppUser appUser)
            throw new ArgumentException("User must be of type AppUser.", nameof(user));
        return appUser;
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
            "DuplicateUserName" => nameof(IUser.UserName),
            "InvalidUserName" => nameof(IUser.UserName),
            "DuplicateEmail" => nameof(IUser.Email),
            "InvalidEmail" => nameof(IUser.Email),
            _ => null,
        };
    }

    public async Task<Result<IUser>> CreateAsync(string email, string userName, string password)
    {
        var user = new AppUser
        {
            UserName = userName,
            Email = email,
        };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            PropertyValidationError[] errors = result.Errors.Select(e => new PropertyValidationError(GetPropertyName(e.Code), e.Code, e.Description)).ToArray();
            return Result.ValidationFailure<IUser>(new ValidationError(errors));
        }
        return Result.Success<IUser>(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(IUser user) =>
        await userManager.GenerateEmailConfirmationTokenAsync(EnsureIsAppUser(user));

    public async Task<string> GenerateEmailChangeTokenAsync(IUser user, string newEmail) =>
        await userManager.GenerateChangeEmailTokenAsync(EnsureIsAppUser(user), newEmail);

    public async Task<Result> ConfirmEmailAsync(IUser user, string token)
    {
        var result = await userManager.ConfirmEmailAsync(EnsureIsAppUser(user), token);
        if (!result.Succeeded)
        {
            PropertyValidationError[] errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
            return Result.ValidationFailure<IUser>(new ValidationError(errors));
        }
        return Result.Success();
    }

    public async Task<Result> ChangeEmailAsync(IUser user, string newEmail, string token)
    {
        var result = await userManager.ChangeEmailAsync(EnsureIsAppUser(user), newEmail, token);
        if (!result.Succeeded)
        {
            PropertyValidationError[] errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
            return Result.ValidationFailure<IUser>(new ValidationError(errors));
        }
        return Result.Success();
    }

    public Task<bool> IsEmailConfirmedAsync(IUser user) =>
        userManager.IsEmailConfirmedAsync(EnsureIsAppUser(user));

    public Task<string> GeneratePasswordResetTokenAsync(IUser user) =>
        userManager.GeneratePasswordResetTokenAsync(EnsureIsAppUser(user));

    public async Task<Result> ResetPasswordAsync(IUser user, string code, string newPassword)
    {
        var result = await userManager.ResetPasswordAsync(EnsureIsAppUser(user), code, newPassword);
        if (!result.Succeeded)
        {
            PropertyValidationError[] errors = result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray();
            return Result.ValidationFailure<IUser>(new ValidationError(errors));
        }
        return Result.Success();
    }
}
