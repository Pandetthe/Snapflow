using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Domain.Members;

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
        Member? member = await dbContext.Members.SingleOrDefaultAsync(m => m.UserId == userId && m.BoardId == boardId);
        HashSet<string> permissionsSet = [];
        if (member == null)
            return permissionsSet;
        if (member.Role == MemberRole.Viewer)
        {
            permissionsSet.Add("View");
        }
        else if (member.Role == MemberRole.Admin)
        {
            permissionsSet.Add("View");
            permissionsSet.Add("Edit");
            permissionsSet.Add("Manage");
        }
        else if (member.Role == MemberRole.Owner)
        {
            permissionsSet.Add("View");
            permissionsSet.Add("Edit");
            permissionsSet.Add("Manage");
            permissionsSet.Add("Delete");
        }
        return permissionsSet;
    }
}
