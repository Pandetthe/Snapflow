namespace Snapflow.Application.Users.Me.TwoFactor.GetStatus;

public sealed record GetTwoFactorStatusResponse(bool IsEnabled, int RecoveryCodesLeft);
