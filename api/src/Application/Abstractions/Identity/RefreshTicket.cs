using System.Security.Claims;

namespace Snapflow.Application.Abstractions.Identity;

public sealed class RefreshTicket
{
    public RefreshTicket(ClaimsPrincipal claimsPrincipal, DateTimeOffset? expiresUtc)
    {
        ClaimsPrincipal = claimsPrincipal;
        ExpiresUtc = expiresUtc;
    }

    public ClaimsPrincipal ClaimsPrincipal { get; }
    public DateTimeOffset? ExpiresUtc { get; }
}
