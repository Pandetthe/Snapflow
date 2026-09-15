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

    Task<Result> UpdateUserNameAsync(IUser user, string userName);

    Task<Result> ChangePasswordAsync(IUser user, string currentPassword, string newPassword);

    Task<Result> UpdateAvatarAsync(IUser user, AvatarType avatarType, byte[]? avatarData, string? contentType);

    Task<Result> SoftDeleteAsync(IUser user, TimeProvider timeProvider);

    Task<IReadOnlyList<string>> GetRolesAsync(IUser user);

    Task<Result> AddToRoleAsync(IUser user, string role);

    Task<Result> RemoveFromRoleAsync(IUser user, string role);

    Task<IUser?> FindByLoginAsync(string provider, string providerKey);

    Task<Result> AddLoginAsync(IUser user, ExternalIdentity identity);

    Task<Result<IUser>> CreateExternalAsync(ExternalIdentity identity, string userName);

    Task<TwoFactorStatus> GetTwoFactorStatusAsync(IUser user);

    Task<AuthenticatorSetup> GetAuthenticatorSetupAsync(IUser user);

    Task<bool> VerifyAuthenticatorCodeAsync(IUser user, string code);

    Task<bool> RedeemRecoveryCodeAsync(IUser user, string recoveryCode);

    Task<Result<IReadOnlyList<string>>> EnableTwoFactorAsync(IUser user);

    Task<Result> DisableTwoFactorAsync(IUser user);

    Task<IReadOnlyList<string>> GenerateRecoveryCodesAsync(IUser user);
}
