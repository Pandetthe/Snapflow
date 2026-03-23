using System.ComponentModel.DataAnnotations;

namespace Snapflow.Infrastructure.Auth;

public sealed class IdentityOptions
{
    public const string SectionName = "IdentityOptions";

    [Range(1, int.MaxValue, ErrorMessage = "ExpiryMinutes must be greater than 0.")]
    public int ExpiryMinutes { get; init; } = 15;

    [Range(1, int.MaxValue, ErrorMessage = "RefreshExpiryMinutes must be greater than 0.")]
    public int RefreshExpiryMinutes { get; init; } = 10080;

    public string? CookieDomain { get; init; }
}
