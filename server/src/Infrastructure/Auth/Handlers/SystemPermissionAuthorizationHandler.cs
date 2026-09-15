using Microsoft.AspNetCore.Authorization;
using Snapflow.Application.Abstractions.Identity;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class SystemPermissionAuthorizationHandler(
    SystemPermissionProvider permissionProvider,
    IUserContext userContext)
    : AuthorizationHandler<SystemPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SystemPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        IReadOnlySet<string> permissions = await permissionProvider.GetForUserIdAsync(userContext.UserId);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
