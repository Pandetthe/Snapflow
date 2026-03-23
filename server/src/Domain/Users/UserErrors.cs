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
    
    public static readonly Error SignOutFailed = Error.Problem(
        "Users.SignOut.Failed",
        "The sign-out attempt failed.");
    
    public static readonly Error AvatarDataMissing = Error.Problem(
        "Users.Avatar.DataMissing",
        "The avatar data is missing in the database, even though the avatar type is set to Custom.");

    public static readonly Error SignInFailed = Error.Unauthorized(
        "Users.SignIn.Failed",
        "The sign-in attempt failed.");

    public static readonly Error SignInLockedOut = Error.Unauthorized(
        "Users.SignIn.LockedOut",
        "The user attempting to sign-in is locked out.");

    public static readonly Error SignInNotAllowed = Error.Unauthorized(
        "Users.SignIn.NotAllowed",
        "The user is not allowed to sign-in due to not confirmed account.");

    public static readonly Error SignInTwoFactorRequired = Error.Unauthorized(
        "Users.SignIn.TwoFactorRequired",
        "The user attempting to sign-in requires two-factor authentication.");

    public static readonly Error PasswordResetInvalidCode = Error.Problem(
        "Users.ResetPassword.InvalidCode",
        "Provided password reset code is invalid.");

    public static readonly Error AvatarFileRequired = Error.Problem(
        "Users.Avatar.FileRequired",
        "Avatar file is required when avatar type is set to Uploaded.");

    public static Error AvatarFileTooLarge(int maxSizeInMB) => Error.Problem(
        "Users.Avatar.FileTooLarge",
        $"Avatar file size exceeds the maximum allowed size of {maxSizeInMB}MB.");

    public static readonly Error AvatarInvalidFileType = Error.Problem(
        "Users.Avatar.InvalidFileType",
        "Avatar file type is not supported. Allowed types: jpg, jpeg, png, gif, webp.");
}
