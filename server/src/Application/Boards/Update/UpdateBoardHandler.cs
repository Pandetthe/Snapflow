using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Update;

internal sealed class UpdateBoardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    IBoardVisibilityPolicy visibilityPolicy,
    TimeProvider timeProvider) : ICommandHandler<UpdateBoardCommand>
{
    public async Task<Result> Handle(UpdateBoardCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(command.Id);

        DateTimeOffset now = timeProvider.GetUtcNow();

        if (command.Visibility is { } visibility && visibility != board.Visibility)
        {
            if (!visibilityPolicy.IsAllowed(visibility))
                return BoardErrors.VisibilityNotAllowed(visibility);

            Result changed = board.ChangeVisibility(visibility, userContext.UserId, now, userContext.ConnectionId);
            if (changed.IsFailure)
                return changed;
        }

        if (command.Members != null)
        {
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
        }

        board.Update(
            command.Title,
            command.Description,
            userContext.UserId,
            now,
            userContext.ConnectionId);

        return await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.BoardMemberKey, MemberErrors.DuplicateMember)],
            cancellationToken);
    }
}