using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.Delete;

internal sealed class DeleteBoardCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<DeleteBoardCommand>
{
    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards.SingleOrDefaultAsync(x => x.Id == command.BoardId, cancellationToken);
        if (board is null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));
        board.IsDeleted = true;
        board.DeletedAt = timeProvider.GetUtcNow();
        board.DeletedById = userContext.UserId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
