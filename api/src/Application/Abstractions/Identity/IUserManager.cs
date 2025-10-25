using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface IUserManager
{
    Task<IUser?> FindByIdAsync(int userId);

    Task<IUser?> FindByEmailAsync(string email);

    Task<IUser?> FindByNameAsync(string userName);

    Task<Result<IUser>> CreateAsync(string email, string userName, string password);

    Task<Result> ConfirmEmailAsync(IUser user, string token);

    Task<Result> ChangeEmailAsync(IUser user, string newEmail, string token);

    Task<string> GenerateEmailConfirmationTokenAsync(IUser user);

    Task<string> GenerateEmailChangeTokenAsync(IUser user, string newEmail);

    Task<bool> IsEmailConfirmedAsync(IUser user);

    Task<string> GeneratePasswordResetTokenAsync(IUser user);

    Task<Result> ResetPasswordAsync(IUser user, string code, string newPassword);
}
