using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Update;

internal sealed class UpdateBoardCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateBoardCommand>
{
    public async Task<Result> Handle(UpdateBoardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
             .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var board = await dbContext.Boards
            .SingleOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);
        if (board == null)
            return Result.Failure(BoardErrors.NotFound(command.Id));

        board.Title = command.Title;
        board.Description = command.Description;
        board.UpdatedAt = timeProvider.GetUtcNow();
        board.UpdatedById = userContext.UserId;
        board.Raise(new BoardUpdatedDomainEvent(board.Id, board.Title, board.Description));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}