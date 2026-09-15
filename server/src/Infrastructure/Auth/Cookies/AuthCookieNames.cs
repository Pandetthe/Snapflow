namespace Snapflow.Infrastructure.Auth.Cookies;

internal static class AuthCookieNames
{
    public const string Session = "Snapflow.Auth.Cookie";
    public const string TwoFactor = "Snapflow.Auth.TwoFactor";
    public const string RememberDevice = "Snapflow.Auth.RememberDevice";
    public const string External = "Snapflow.Auth.External";
    public const string CorrelationPrefix = "Snapflow.Auth.Correlation.";
    public const string NoncePrefix = "Snapflow.Auth.Nonce.";
}
