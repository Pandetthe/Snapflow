using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.TwoFactor;

public sealed record TwoFactorCommand(bool? Enable, string? TwoFactorCode, bool ResetSharedKey, bool ResetRecoveryCodes, bool ForgetMachine) : ICommand;