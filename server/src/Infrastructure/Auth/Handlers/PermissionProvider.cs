using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class PermissionProvider(IAppDbContext dbContext)
{
    private static readonly Dictionary<MemberRole, HashSet<string>> _permissionsByRole = new()
    {
        [MemberRole.Owner] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Boards.Update,
            BoardPermissions.Boards.Delete,
            BoardPermissions.Swimlanes.Create,
            BoardPermissions.Swimlanes.Update,
            BoardPermissions.Swimlanes.Delete,
            BoardPermissions.Swimlanes.Move,
            BoardPermissions.Lists.Create,
            BoardPermissions.Lists.Update,
            BoardPermissions.Lists.Delete,
            BoardPermissions.Lists.Move,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move

        },
        [MemberRole.Admin] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Boards.Update,
            BoardPermissions.Swimlanes.Create,
            BoardPermissions.Swimlanes.Update,
            BoardPermissions.Swimlanes.Delete,
            BoardPermissions.Swimlanes.Move,
            BoardPermissions.Lists.Create,
            BoardPermissions.Lists.Update,
            BoardPermissions.Lists.Delete,
            BoardPermissions.Lists.Move,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move

        },
        [MemberRole.Member] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move
        },
        [MemberRole.Viewer] = new()
        {
            BoardPermissions.Boards.View,
        },
    };
    public async Task<HashSet<string>> GetForUserIdAsync(int userId, int boardId)
    {
        MemberRole? role = await dbContext.Members
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.BoardId == boardId)
            .Select(m => (MemberRole?)m.Role)
            .SingleOrDefaultAsync();
        if (role.HasValue && _permissionsByRole.TryGetValue(role.Value, out var permissions))
            return permissions;
        return [];
    }
}
