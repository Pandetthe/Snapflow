using System.Security.Claims;

namespace Snapflow.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int parsedUserId) ? parsedUserId :
            throw new InvalidOperationException("User id is unavailable");
    }
}
