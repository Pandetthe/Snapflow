using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Snapflow.Application.Abstractions.Identity;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class UserPermissionAuthorizationHandler(
    IServiceScopeFactory serviceScopeFactory,
    IUserContext userContext) : AuthorizationHandler<UserPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserPermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

        int userId = userContext.UserId;

        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
