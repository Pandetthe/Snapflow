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
        Member? oldOwner = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.Role == MemberRole.Owner, cancellationToken);
        if (oldOwner == null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));
        if (oldOwner.UserId == command.UserId)
            return Result.Success();
        Member? newOwner = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.UserId == command.UserId, cancellationToken);
        if (newOwner == null)
            return Result.Failure(MemberErrors.NotFound(command.UserId, command.BoardId));
        oldOwner.UpdateRole(MemberRole.Admin, userContext.ConnectionId);
        await dbContext.SaveChangesAsync(cancellationToken);

        newOwner.UpdateRole(MemberRole.Owner, userContext.ConnectionId);
        return await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.BoardSingleOwner, MemberErrors.OwnerAlreadyExists(command.BoardId))],
            cancellationToken);
    }
}