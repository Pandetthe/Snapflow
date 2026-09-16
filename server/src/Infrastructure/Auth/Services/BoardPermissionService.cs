using Snapflow.Application.Abstractions.Identity;
using Snapflow.Infrastructure.Authorization;

namespace Snapflow.Infrastructure.Auth.Services;

internal sealed class BoardPermissionService(
    PermissionProvider permissionProvider,
    IUserContext userContext) : IBoardPermissionService
{
    public async Task<bool> HasPermissionAsync(int boardId, string permission, CancellationToken cancellationToken = default)
    {
        int? userId = userContext.IsAuthenticated ? userContext.UserId : null;
        IReadOnlySet<string> permissions = await permissionProvider.GetForUserIdAsync(userId, boardId);
        return permissions.Contains(permission);
    }
}
