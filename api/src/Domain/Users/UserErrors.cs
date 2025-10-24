using Snapflow.Common;

namespace Snapflow.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(int userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found.");

    public static readonly Error Unauthorized = Error.Unauthorized(
        "Users.Unauthorized",
        "User is not authorized to perform this action.");

    public static readonly Error RefreshFailed = Error.Unauthorized(
        "Users.Refresh.Unauthorized",
        "The token refresh attempt failed.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found.");

    public static readonly Error SignInFailed = Error.Failure(
        "Users.SignIn.Failed",
        "The sign-in attempt failed.");

    public static readonly Error SignInLockedOut = Error.Failure(
        "Users.SignIn.LockedOut",
        "The user attempting to sign-in is locked out.");

    public static readonly Error SignInNotAllowed = Error.Failure(
        "Users.SignIn.NotAllowed",
        "The user attempting to sign-in is not allowed to sign-in.");

    public static readonly Error SignInTwoFactorRequired = Error.Failure(
        "Users.SignIn.TwoFactorRequired",
        "The user attempting to sign-in requires two-factor authentication.");
}
