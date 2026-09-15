namespace Snapflow.Application.Abstractions.Identity;

public sealed record TwoFactorStatus(bool IsEnabled, int RecoveryCodesLeft);

public sealed record AuthenticatorSetup(string SharedKey, string AuthenticatorUri);
