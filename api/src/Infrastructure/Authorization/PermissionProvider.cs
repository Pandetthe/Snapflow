using Snapflow.Application.Abstractions.Persistence;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class PermissionProvider()
{
    HashSet<string> permissionsSet = [];

    public Task<HashSet<string>> GetForUserIdAsync(int userId)
    {

        return Task.FromResult(permissionsSet);
    }

    public Task<HashSet<string>> GetForUserIdAsync(int userId, int boardId)
    {
        return Task.FromResult(permissionsSet);
    }
}
