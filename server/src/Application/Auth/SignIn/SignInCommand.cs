using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.SignIn;

public sealed record SignInCommand(
    string Email,
    string Password,
    string? TwoFactorCode,
    string? TwoFactorRecoveryCode,
    bool? UseCookies,
    bool? UseSessionCookies) : ICommand;