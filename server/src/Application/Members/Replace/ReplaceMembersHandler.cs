using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Members.Replace;

internal sealed class ReplaceMembersHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<ReplaceMembersCommand>
{
    public async Task<Result> Handle(ReplaceMembersCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board is null)
            return BoardErrors.NotFound(command.BoardId);

        var memberUserIds = command.Members.Select(m => m.UserId).Distinct().ToList();
        var existingUserIds = await dbContext.Users
            .AsNoTracking()
            .Where(u => memberUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var missingUserId = memberUserIds.FirstOrDefault(id => !existingUserIds.Contains(id));
        if (missingUserId != 0)
            return UserErrors.NotFound(missingUserId);

        Result synced = board.SyncMembers(
            command.Members.Select(m => (m.UserId, m.Role)).ToList(),
            userContext.ConnectionId);
        if (synced.IsFailure)
            return synced;

        return await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.BoardMemberKey, MemberErrors.DuplicateMember)],
            cancellationToken);
    }
}