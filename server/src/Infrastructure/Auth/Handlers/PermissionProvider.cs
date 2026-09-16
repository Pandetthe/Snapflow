using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class PermissionProvider(IAppDbContext dbContext, IBoardVisibilityPolicy visibilityPolicy)
{
    private static readonly HashSet<string> _nonMemberPermissions =
    [
        BoardPermissions.Boards.View
    ];

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
            BoardPermissions.Cards.Move,
            BoardPermissions.Tags.Create,
            BoardPermissions.Tags.Update,
            BoardPermissions.Tags.Delete,
            BoardPermissions.Tags.Assign,
            BoardPermissions.Boards.Transfer,
            BoardPermissions.Boards.ChangeVisibility
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
            BoardPermissions.Cards.Move,
            BoardPermissions.Tags.Create,
            BoardPermissions.Tags.Update,
            BoardPermissions.Tags.Delete,
            BoardPermissions.Tags.Assign
        },
        [MemberRole.Member] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move,
            BoardPermissions.Tags.Assign
        },
        [MemberRole.Viewer] = new()
        {
            BoardPermissions.Boards.View,
        },
    };
    public async Task<IReadOnlySet<string>> GetForUserIdAsync(int? userId, int boardId)
    {
        if (userId is not null)
        {
            MemberRole? role = await dbContext.Members
                .AsNoTracking()
                .Where(m => m.UserId == userId && m.BoardId == boardId)
                .Select(m => (MemberRole?)m.Role)
                .SingleOrDefaultAsync();
            if (role.HasValue && _permissionsByRole.TryGetValue(role.Value, out var permissions))
                return permissions;
        }

        BoardVisibility? visibility = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == boardId && !b.IsDeleted)
            .Select(b => (BoardVisibility?)b.Visibility)
            .SingleOrDefaultAsync();
        if (visibility.HasValue && visibilityPolicy.CanNonMemberView(visibility.Value, userId is not null))
            return _nonMemberPermissions;
        return new HashSet<string>();
    }
}
