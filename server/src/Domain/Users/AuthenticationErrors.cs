using Snapflow.Common;

namespace Snapflow.Domain.Users;

public static class AuthenticationErrors
{
    public static readonly Error PasswordAuthenticationDisabled = Error.NotFound(
        "Users.PasswordAuthentication.Disabled",
        "Signing in and signing up with a password is disabled. Use your organization's sign-in instead.");

    public static readonly Error ProviderNotAvailable = Error.NotFound(
        "Users.External.ProviderNotAvailable",
        "The requested sign-in provider is not available.");

    public static readonly Error ExternalSignInFailed = Error.Unauthorized(
        "Users.External.Failed",
        "The sign-in with the external provider failed.");

    public static readonly Error ExternalEmailMissing = Error.Unauthorized(
        "Users.External.EmailMissing",
        "The sign-in provider did not share an email address.");

    public static readonly Error ExternalEmailNotVerified = Error.Unauthorized(
        "Users.External.EmailNotVerified",
        "An account with this email already exists, but the sign-in provider has not verified the email, so it cannot be linked.");

    public static readonly Error ExistingAccountNotConfirmed = Error.Unauthorized(
        "Users.External.AccountNotConfirmed",
        "An account with this email already exists but its email is not confirmed. Confirm it or sign in with its password first.");

    public static readonly Error ExternalSignUpDisabled = Error.Unauthorized(
        "Users.External.SignUpDisabled",
        "There is no account for this sign-in and new accounts cannot be created with it.");

    public static readonly Error ExternalConfirmationSent = Error.Unauthorized(
        "Users.External.ConfirmationSent",
        "Your account was created. Confirm your email address with the link we sent before signing in.");
}
