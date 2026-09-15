using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.TwoFactor.RegenerateRecoveryCodes;

public sealed record RegenerateRecoveryCodesCommand(string Code) : ICommand<RegenerateRecoveryCodesResponse>;
