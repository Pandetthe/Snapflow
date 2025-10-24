using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? TwoFactorCode,
    string? TwoFactorRecoveryCode,
    bool? UseCookies,
    bool? UseSessionCookies) : ICommand;