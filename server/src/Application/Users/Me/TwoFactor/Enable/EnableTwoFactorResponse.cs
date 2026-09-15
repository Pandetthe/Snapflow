namespace Snapflow.Application.Users.Me.TwoFactor.Enable;

public sealed record EnableTwoFactorResponse(IReadOnlyList<string> RecoveryCodes);
