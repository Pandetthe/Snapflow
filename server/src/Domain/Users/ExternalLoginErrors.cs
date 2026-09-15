using Snapflow.Common;

namespace Snapflow.Domain.Users;

public static class ExternalLoginErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Users.Logins.NotFound",
        "This account is not connected.");

    public static readonly Error LinkFailed = Error.Problem(
        "Users.Logins.LinkFailed",
        "The account could not be connected. Try again.");

    public static readonly Error LinkedToAnotherAccount = Error.Conflict(
        "Users.Logins.LinkedToAnotherAccount",
        "This account is already connected to a different user.");

    public static readonly Error ProviderAlreadyLinked = Error.Conflict(
        "Users.Logins.ProviderAlreadyLinked",
        "An account from this provider is already connected. Disconnect it before connecting another one.");

    public static readonly Error LastSignInMethod = Error.Problem(
        "Users.Logins.LastSignInMethod",
        "This is your only way to sign in. Set a password, add a passkey or connect another account before disconnecting it.");
}
