using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Delete;

internal sealed class DeleteBoardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteBoardCommand>
{
    public Task<Result> Handle(DeleteBoardCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result> ExecuteAsync(DeleteBoardCommand command, CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return UserErrors.NotFound(userContext.UserId);

        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(x => x.Id == command.BoardId, cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(command.BoardId);

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        var userId = userContext.UserId;

        board.SoftDelete(userId, dateTimeOffset, userContext.ConnectionId);

        await dbContext.Swimlanes
            .Where(s => s.BoardId == board.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, dateTimeOffset)
                .SetProperty(x => x.DeletedById, userId)
                .SetProperty(x => x.DeletedByCascade, true),
                cancellationToken);

        await dbContext.Lists
            .Where(l => l.BoardId == board.Id)
            .ExecuteUpdateAsync(l => l
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, dateTimeOffset)
                .SetProperty(x => x.DeletedById, userId)
                .SetProperty(x => x.DeletedByCascade, true),
                cancellationToken);

        await dbContext.Cards
            .Where(c => c.BoardId == board.Id)
            .ExecuteUpdateAsync(c => c
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, dateTimeOffset)
                .SetProperty(x => x.DeletedById, userId)
                .SetProperty(x => x.DeletedByCascade, true),
                cancellationToken);

        await dbContext.Tags
            .Where(t => t.BoardId == board.Id)
            .ExecuteUpdateAsync(t => t
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, dateTimeOffset)
                .SetProperty(x => x.DeletedById, userId),
                cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
