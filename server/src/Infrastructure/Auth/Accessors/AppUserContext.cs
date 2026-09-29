using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;

namespace Snapflow.Infrastructure.Auth.Accessors;

internal sealed class AppUserContext(
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    HubCallerContextAccessor hubCallerContextAccessor) : IUserContext
{
    public int UserId
    {
        get
        {
            var userId = userManager.GetUserId(Principal);
            return int.TryParse(userId, out var id)
                ? id
                : throw new InvalidOperationException("User identifier is not available.");
        }
    }

    public string UserName =>
        userManager.GetUserName(Principal)
        ?? throw new InvalidOperationException("UserName is not available.");

    public async Task<IUser> GetUserAsync()
    {
        AppUser? user = await userManager.GetUserAsync(Principal);
        return user ?? throw new InvalidOperationException("User is not available.");
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated
        ?? hubCallerContextAccessor.HubCallerContext?.User?.Identity?.IsAuthenticated
        ?? false;

    public string? ConnectionId => hubCallerContextAccessor.ConnectionId;

    private ClaimsPrincipal Principal =>
        httpContextAccessor.HttpContext?.User
        ?? hubCallerContextAccessor.HubCallerContext?.User
        ?? throw new InvalidOperationException("No authentication context available.");
}
