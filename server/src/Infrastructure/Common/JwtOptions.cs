namespace Snapflow.Infrastructure.Common;

public sealed class JwtOptions
{
    public string Secret { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int ExpirationInMinutes { get; init; } = 15;
    public string? CookieDomain { get; init; }
}
