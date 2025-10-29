using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class PermissionProvider(IAppDbContext dbContext)
{
#pragma warning disable CA1822 // Mark members as static
    public Task<HashSet<string>> GetForUserIdAsync(int userId)
#pragma warning restore CA1822 // Mark members as static
    {

        return Task.FromResult(new HashSet<string>());
    }

    public async Task<HashSet<string>> GetForUserIdAsync(int userId, int boardId)
    {
        BoardMember? member = await dbContext.BoardMembers.SingleOrDefaultAsync(m => m.UserId == userId && m.BoardId == boardId);
        HashSet<string> permissionsSet = [];
        if (member == null)
            return permissionsSet;
        if (member.Role == BoardMemberRole.Viewer)
        {
            permissionsSet.Add("View");
        }
        else if (member.Role == BoardMemberRole.Admin)
        {
            permissionsSet.Add("View");
            permissionsSet.Add("Edit");
            permissionsSet.Add("Manage");
        }
        else if (member.Role == BoardMemberRole.Owner)
        {
            permissionsSet.Add("View");
            permissionsSet.Add("Edit");
            permissionsSet.Add("Manage");
            permissionsSet.Add("Delete");
        }
        return permissionsSet;
    }
}
