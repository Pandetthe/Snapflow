using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.TwoFactorSignIn;

public sealed record TwoFactorSignInCommand(
    string? Code,
    string? RecoveryCode,
    string? PasskeyCredential,
    string? PasskeyState,
    bool RememberDevice,
    string? TwoFactorToken,
    bool? UseCookies,
    bool? UseSessionCookies) : ICommand;
