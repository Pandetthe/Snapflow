using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.ChangeVisibility;

internal sealed class ChangeBoardVisibilityHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    IBoardVisibilityPolicy visibilityPolicy,
    TimeProvider timeProvider) : ICommandHandler<ChangeBoardVisibilityCommand>
{
    public async Task<Result> Handle(ChangeBoardVisibilityCommand command, CancellationToken cancellationToken = default)
    {
        if (!visibilityPolicy.IsAllowed(command.Visibility))
            return Result.Failure(BoardErrors.VisibilityNotAllowed(command.Visibility));

        Board? board = await dbContext.Boards
            .SingleOrDefaultAsync(b => b.Id == command.Id && !b.IsDeleted, cancellationToken);
        if (board == null)
            return Result.Failure(BoardErrors.NotFound(command.Id));

        board.ChangeVisibility(
            command.Visibility,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
