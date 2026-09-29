using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class PermissionProvider(IAppDbContext dbContext, IBoardVisibilityPolicy visibilityPolicy)
{
    public async Task<IReadOnlySet<string>> GetForUserIdAsync(int? userId, int boardId)
    {
        if (userId is not null)
        {
            MemberRole? role = await dbContext.Members
                .AsNoTracking()
                .IgnoreQueryFilters([ISoftDeletable.FilterName])
                .Where(m => m.UserId == userId && m.BoardId == boardId)
                .Select(m => (MemberRole?)m.Role)
                .SingleOrDefaultAsync();
            if (role.HasValue)
                return MemberRolePermissions.For(role.Value);
        }

        BoardVisibility? visibility = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == boardId)
            .Select(b => (BoardVisibility?)b.Visibility)
            .SingleOrDefaultAsync();
        if (visibility.HasValue && visibilityPolicy.CanNonMemberView(visibility.Value, userId is not null))
            return MemberRolePermissions.NonMember;
        return new HashSet<string>();
    }
}
