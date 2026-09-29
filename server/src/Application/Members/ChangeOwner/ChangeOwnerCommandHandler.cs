using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.ChangeOwner;

internal sealed class ChangeOwnerCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<ChangeOwnerCommand>
{
    public Task<Result> Handle(ChangeOwnerCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result> ExecuteAsync(ChangeOwnerCommand command, CancellationToken cancellationToken)
    {
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board is null)
            return BoardErrors.NotFound(command.BoardId);
        if (board.IsOwnedBy(command.UserId))
            return Result.Success();

        Result handedOver = board.HandOverOwnership(command.UserId, userContext.ConnectionId);
        if (handedOver.IsFailure)
            return handedOver;

        await dbContext.SaveChangesAsync(cancellationToken);

        Result taken = board.TakeOwnership(command.UserId, userContext.ConnectionId);
        if (taken.IsFailure)
            return taken;

        return await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.BoardSingleOwner, MemberErrors.OwnerAlreadyExists(command.BoardId))],
            cancellationToken);
    }
}