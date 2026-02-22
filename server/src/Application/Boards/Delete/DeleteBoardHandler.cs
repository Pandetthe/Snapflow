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
    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        var board = await dbContext.Boards
            .SingleOrDefaultAsync(x => x.Id == command.BoardId && !x.IsDeleted, cancellationToken);
        if (board == null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            board.SoftDelete(userId, dateTimeOffset, userContext.ConnectionId);

            await dbContext.Swimlanes
                .Where(s => s.BoardId == board.Id && !s.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId)
                    .SetProperty(x => x.DeletedByCascade, true),
                    cancellationToken);

            await dbContext.Lists
                .Where(l => l.BoardId == board.Id && !l.IsDeleted)
                .ExecuteUpdateAsync(l => l
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId)
                    .SetProperty(x => x.DeletedByCascade, true),
                    cancellationToken);

            await dbContext.Cards
                .Where(c => c.BoardId == board.Id && !c.IsDeleted)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId)
                    .SetProperty(x => x.DeletedByCascade, true),
                    cancellationToken);

            await dbContext.Tags
                .Where(t => t.BoardId == board.Id && !t.IsDeleted)
                .ExecuteUpdateAsync(t => t
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId),
                    cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Result.Success();
    }
}
