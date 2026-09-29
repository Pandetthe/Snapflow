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
    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .Include(b => b.Swimlanes)
            .Include(b => b.Lists)
            .Include(b => b.Cards)
            .Include(b => b.Tags)
            .AsSplitQuery()
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(command.BoardId);

        board.SoftDelete(userContext.UserId, timeProvider.GetUtcNow(), userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
