using Microsoft.EntityFrameworkCore;
using Snapflow.Domain.Roles;
using Snapflow.Infrastructure.Persistence;

namespace Snapflow.Infrastructure.Authorization;

// Reads the roles from the database on every request rather than from the principal's role claims,
// so taking a role away takes effect immediately instead of when the cookie or token is reissued.
internal sealed class SystemPermissionProvider(AppDbContext dbContext)
{
    // One request can check several policies; the roles are only looked up once per user.
    private readonly Dictionary<int, IReadOnlySet<string>> _cache = [];

    public async Task<IReadOnlySet<string>> GetForUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(userId, out IReadOnlySet<string>? cached))
            return cached;

        List<string> roles = await dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(dbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
            .ToListAsync(cancellationToken);

        HashSet<string> permissions = SystemRolePermissions.For(roles);
        _cache[userId] = permissions;
        return permissions;
    }
}
