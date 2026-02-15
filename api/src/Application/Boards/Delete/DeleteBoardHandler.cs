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
            .Include(b => b.Swimlanes.Where(s => !s.IsDeleted))
            .Include(b => b.Lists.Where(l => !l.IsDeleted))
            .Include(b => b.Cards.Where(c => !c.IsDeleted))
            .SingleOrDefaultAsync(x => x.Id == command.BoardId && !x.IsDeleted, cancellationToken);
        if (board == null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        board.IsDeleted = true;
        board.DeletedAt = dateTimeOffset;
        board.DeletedById = userId;

        // TODO: Optimize to not load entire swimlanes and lists into memory
        //       Domain events for now cannot be raised without loading and
        //       and modifying tracked entity.

        // TODO: Consider soft deleting Tags associated with Board

        foreach (var swimlane in board.Swimlanes)
        {
            swimlane.IsDeleted = true;
            swimlane.DeletedAt = dateTimeOffset;
            swimlane.DeletedById = userId;
            swimlane.DeletedByCascade = true;
        }
        foreach (var list in board.Lists)
        {
            list.IsDeleted = true;
            list.DeletedAt = dateTimeOffset;
            list.DeletedById = userId;
            list.DeletedByCascade = true;
        }
        foreach (var card in board.Cards)
        {
            card.IsDeleted = true;
            card.DeletedAt = dateTimeOffset;
            card.DeletedById = userId;
            card.DeletedByCascade = true;
        }

        board.Raise((entity) => new BoardDeletedDomainEvent(entity.Id, userContext.ConnectionId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
