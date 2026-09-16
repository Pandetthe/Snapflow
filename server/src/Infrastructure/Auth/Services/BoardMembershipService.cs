using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Persistence;

namespace Snapflow.Infrastructure.Auth.Services;

internal sealed class BoardMembershipService(
    IAppDbContext dbContext,
    IUserContext userContext) : IBoardMembershipService
{
    public async Task<bool> IsMemberAsync(int boardId, CancellationToken cancellationToken = default)
    {
        if (!userContext.IsAuthenticated)
            return false;

        int userId = userContext.UserId;
        return await dbContext.Members
            .AsNoTracking()
            .AnyAsync(m => m.BoardId == boardId && m.UserId == userId, cancellationToken);
    }
}
