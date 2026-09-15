namespace Snapflow.Application.Users.Me.TwoFactor.SetupAuthenticator;

public sealed record SetupAuthenticatorResponse(string SharedKey, string AuthenticatorUri);
