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
            if (httpContextAccessor.HttpContext == null)
                throw new InvalidOperationException("No http context available.");
            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            return int.TryParse(userId, out var id) ? id : throw new InvalidOperationException("User identifier is not available.");
        }
    }

    public string UserName
    {
        get
        {
            if (httpContextAccessor.HttpContext == null)
                throw new InvalidOperationException("No http context available.");
            var userName = userManager.GetUserName(httpContextAccessor.HttpContext.User)
                           ?? throw new InvalidOperationException("UserName is not available.");
            return userName;
        }
    }

    public async Task<IUser> GetUserAsync()
    {
        if (httpContextAccessor.HttpContext == null)
            throw new InvalidOperationException("No http context available.");
        AppUser? user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
        return user ?? throw new InvalidOperationException("User is not available.");
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? ConnectionId => hubCallerContextAccessor.ConnectionId;
}
