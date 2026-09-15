using Snapflow.Application.Abstractions.Identity;
using Snapflow.Infrastructure.Authorization;

namespace Snapflow.Infrastructure.Auth.Services;

internal sealed class SystemPermissionService(
    SystemPermissionProvider permissionProvider,
    IUserContext userContext) : ISystemPermissionService
{
    public async Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        IReadOnlySet<string> permissions = await GetPermissionsAsync(cancellationToken);
        return permissions.Contains(permission);
    }

    public Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (!userContext.IsAuthenticated)
            return Task.FromResult<IReadOnlySet<string>>(new HashSet<string>());

        return permissionProvider.GetForUserIdAsync(userContext.UserId, cancellationToken);
    }
}
