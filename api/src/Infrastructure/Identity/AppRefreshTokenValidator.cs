using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Identity.Entities;
using System.Security.Claims;

namespace Snapflow.Infrastructure.Identity;

internal sealed class AppRefreshTokenValidator(
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    SignInManager<AppUser> signInManager,
    TimeProvider timeProvider) : IRefreshTokenValidator
{
    public bool IsTokenExpired(DateTimeOffset? expiresUtc) =>
        expiresUtc == null || timeProvider.GetUtcNow() >= expiresUtc;

    public RefreshTicket? UnprotectRefreshToken(string refreshToken)
    {
        var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var ticket = refreshTokenProtector?.Unprotect(refreshToken);
        if (ticket == null)
            return null;
        return new RefreshTicket(ticket.Principal, ticket.Properties.ExpiresUtc);
    }

    public async Task<IUser?> ValidateSecurityStampAsync(ClaimsPrincipal? principal) =>
        await signInManager.ValidateSecurityStampAsync(principal);
}
