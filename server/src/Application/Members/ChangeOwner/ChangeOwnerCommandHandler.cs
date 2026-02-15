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
    public async Task<Result> Handle(ChangeOwnerCommand command, CancellationToken cancellationToken = default)
    {
        var oldOwner = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.Role == MemberRole.Owner, cancellationToken);
        if (oldOwner == null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));
        var newOwner = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.UserId == command.UserId, cancellationToken);
        if (newOwner == null)
            return Result.Failure(MemberErrors.NotFound(command.UserId, command.BoardId));
        oldOwner.Raise((entity) => new MemberRoleChangedDomainEvent(entity.UserId,
            entity.BoardId, oldOwner.Role, MemberRole.Admin, userContext.ConnectionId));
        newOwner.Raise((entity) => new MemberRoleChangedDomainEvent(entity.UserId,
            entity.BoardId, newOwner.Role, MemberRole.Owner, userContext.ConnectionId));
        oldOwner.Role = MemberRole.Admin;
        newOwner.Role = MemberRole.Owner;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}