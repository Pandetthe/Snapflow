namespace Snapflow.Infrastructure.Auth.Managers;

internal static class TwoFactorCode
{
    public static string NormalizeAuthenticatorCode(string code) =>
        code.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);

    public static string NormalizeRecoveryCode(string code) =>
        code.Replace(" ", string.Empty, StringComparison.Ordinal);
}
