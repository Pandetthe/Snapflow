namespace Snapflow.Application.Users.Me.TwoFactor.RegenerateRecoveryCodes;

public sealed record RegenerateRecoveryCodesResponse(IReadOnlyList<string> RecoveryCodes);
