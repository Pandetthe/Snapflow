using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

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
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(command.BoardId);

        DateTimeOffset deletedAt = timeProvider.GetUtcNow();
        board.SoftDelete(userContext.UserId, deletedAt, userContext.ConnectionId);
        await dbContext.CascadeBoardDeletionAsync(board.Id, userContext.UserId, deletedAt, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
