using Snapflow.Common;

namespace Snapflow.Domain.Users;

public static class PasskeyErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Users.Passkeys.NotFound",
        "The passkey was not found.");

    public static readonly Error RegistrationFailed = Error.Problem(
        "Users.Passkeys.RegistrationFailed",
        "The passkey could not be added. Try again.");

    public static readonly Error AlreadyRegistered = Error.Conflict(
        "Users.Passkeys.AlreadyRegistered",
        "This passkey is already added to an account.");

    public static readonly Error LimitReached = Error.Problem(
        "Users.Passkeys.LimitReached",
        $"You can add up to {UserOptions.MaxPasskeysPerUser} passkeys. Remove one before adding another.");

    public static readonly Error NotRecognized = Error.Unauthorized(
        "Users.Passkeys.NotRecognized",
        "The passkey was not recognized. Try another passkey or sign in another way.");

    public static readonly Error Expired = Error.Problem(
        "Users.Passkeys.Expired",
        "The passkey request expired. Try again.");

    public static readonly Error NoneRegistered = Error.Problem(
        "Users.Passkeys.NoneRegistered",
        "There are no passkeys on this account.");
}
