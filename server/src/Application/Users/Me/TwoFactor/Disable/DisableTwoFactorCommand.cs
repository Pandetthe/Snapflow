using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.TwoFactor.Disable;

public sealed record DisableTwoFactorCommand(string? Code, string? RecoveryCode) : ICommand;
