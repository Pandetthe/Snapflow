using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Snapflow.Application.Abstractions.Identity;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class BoardPermissionAuthorizationHandler(
    IServiceScopeFactory serviceScopeFactory,
    IUserContext userContext) : AuthorizationHandler<BoardPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BoardPermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;
        if (context.Resource is not HttpContext httpContext)
            return;
        var boardIdStr = httpContext.Request.RouteValues["boardId"]?.ToString();
        if (!int.TryParse(boardIdStr, out int boardId))
            return;

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

        int userId = userContext.UserId;

        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId, boardId);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
