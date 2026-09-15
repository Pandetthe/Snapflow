using Snapflow.Common;

namespace Snapflow.Domain.Users;

public static class TwoFactorErrors
{
    public static readonly Error InvalidCode = Error.Problem(
        "Users.TwoFactor.InvalidCode",
        "The code is not valid. Enter the current code from your authenticator app.");

    public static readonly Error AlreadyEnabled = Error.Conflict(
        "Users.TwoFactor.AlreadyEnabled",
        "Two-factor authentication is already turned on.");

    public static readonly Error NotEnabled = Error.Problem(
        "Users.TwoFactor.NotEnabled",
        "Two-factor authentication is not turned on.");

    public static readonly Error SignInExpired = Error.Unauthorized(
        "Users.TwoFactor.SignInExpired",
        "The sign-in expired before the code was entered. Sign in again.");
}
