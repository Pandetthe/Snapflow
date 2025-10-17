using Microsoft.AspNetCore.Http;
using Snapflow.Application.Abstractions.Authentication;

namespace Snapflow.Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId => _httpContextAccessor
        .HttpContext?.User.GetUserId()
        ?? throw new InvalidOperationException("User context is unavailable");
}
