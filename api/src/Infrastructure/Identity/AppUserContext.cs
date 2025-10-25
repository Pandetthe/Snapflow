using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Identity.Entities;

namespace Snapflow.Infrastructure.Identity;

internal sealed class AppUserContext(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public int UserId
    {
        get
        {
            if (httpContextAccessor.HttpContext == null)
                throw new InvalidOperationException("No http context available.");
            string? userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            if (int.TryParse(userId, out int id))
                return id;
            throw new InvalidOperationException("User identifier is not available.");
        }
    }

    public string UserName
    {
        get
        {
            if (httpContextAccessor.HttpContext == null)
                throw new InvalidOperationException("No http context available.");
            string userName = userManager.GetUserName(httpContextAccessor.HttpContext.User)
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
}
