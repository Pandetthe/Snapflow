using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Update;

internal sealed class UpdateBoardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    IBoardPermissionService permissionService,
    IBoardVisibilityPolicy visibilityPolicy,
    TimeProvider timeProvider) : ICommandHandler<UpdateBoardCommand>
{
    public async Task<Result> Handle(UpdateBoardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
             .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        Board? board = await dbContext.Boards
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);
        if (board == null)
            return Result.Failure(BoardErrors.NotFound(command.Id));

        bool changesVisibility = command.Visibility is not null && command.Visibility != board.Visibility;
        if (changesVisibility)
        {
            if (!visibilityPolicy.IsAllowed(command.Visibility!.Value))
                return Result.Failure(BoardErrors.VisibilityNotAllowed(command.Visibility.Value));

            if (!await permissionService.HasPermissionAsync(board.Id, BoardPermissions.Boards.ChangeVisibility, cancellationToken))
                return Result.Failure(BoardErrors.VisibilityChangeForbidden(board.Id));
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
                return Result.Failure(UserErrors.NotFound(missingUserId));

            board.SyncMembers(
                command.Members.Select(m => (m.UserId, m.Role)).ToList(),
                userContext.ConnectionId);
        }

        board.Update(
            command.Title,
            command.Description,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        if (changesVisibility)
        {
            board.ChangeVisibility(
                command.Visibility!.Value,
                userContext.UserId,
                timeProvider.GetUtcNow(),
                userContext.ConnectionId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}