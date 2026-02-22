using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.ChangeRole;

internal sealed class ChangeMemberRoleCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<ChangeMemberRoleCommand>
{
    public async Task<Result> Handle(ChangeMemberRoleCommand command, CancellationToken cancellationToken = default)
    {
        var member = await dbContext.Members.SingleOrDefaultAsync(
            x => x.BoardId == command.BoardId && x.UserId == command.UserId, cancellationToken);
        if (member == null)
            return Result.Failure(MemberErrors.NotFound(command.UserId, command.BoardId));
        member.UpdateRole(command.Role, userContext.ConnectionId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}