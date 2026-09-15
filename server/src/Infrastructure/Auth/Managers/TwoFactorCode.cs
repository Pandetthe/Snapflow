namespace Snapflow.Infrastructure.Auth.Managers;

internal static class TwoFactorCode
{
    private const int RecoveryCodeLength = 10;

    public static string NormalizeAuthenticatorCode(string code) =>
        code.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);

    public static string NormalizeRecoveryCode(string code)
    {
        string compact = new string(code.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        return compact.Length == RecoveryCodeLength
            ? $"{compact[..(RecoveryCodeLength / 2)]}-{compact[(RecoveryCodeLength / 2)..]}"
            : compact;
    }
}
