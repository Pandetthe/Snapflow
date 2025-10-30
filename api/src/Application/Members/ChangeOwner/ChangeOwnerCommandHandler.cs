using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Members;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Members.ChangeOwner;

internal sealed class ChangeOwnerCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<ChangeOwnerCommand>
{
    public async Task<Result> Handle(ChangeOwnerCommand command, CancellationToken cancellationToken = default)
    {
        var boardOwner = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.Role == MemberRole.Owner, cancellationToken);
        if (boardOwner == null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));
        var newOwner = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.UserId == command.UserId, cancellationToken);
        if (newOwner == null)
            return Result.Failure(MemberErrors.NotFound(command.UserId, command.BoardId));
        boardOwner.Raise(new MemberRoleChangedDomainEvent(boardOwner.UserId, 
            boardOwner.BoardId, boardOwner.Role, MemberRole.Admin));
        newOwner.Raise(new MemberRoleChangedDomainEvent(newOwner.UserId,
            newOwner.BoardId, newOwner.Role, MemberRole.Owner));
        boardOwner.Role = MemberRole.Admin;
        newOwner.Role = MemberRole.Owner;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}