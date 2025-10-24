using Snapflow.Domain.Users;
using System.Security.Claims;

namespace Snapflow.Application.Abstractions.Identity;

public interface IRefreshTokenValidator
{
    RefreshTicket? UnprotectRefreshToken(string refreshToken);

    bool IsTokenExpired(DateTimeOffset? expiresUtc);

    Task<IUser?> ValidateSecurityStampAsync(ClaimsPrincipal? principal);
}
