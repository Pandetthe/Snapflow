using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Application.BoardMembers.ChangeRole;

internal sealed class ChangeBoardMemberRoleCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<ChangeBoardMemberRoleCommand>
{
    public async Task<Result> Handle(ChangeBoardMemberRoleCommand command, CancellationToken cancellationToken = default)
    {
        var member = await dbContext.BoardMembers.SingleOrDefaultAsync(
            x => x.BoardId == command.BoardId && x.UserId == command.UserId, cancellationToken);
        if (member == null)
            return Result.Failure(BoardMemberErrors.NotFound(command.UserId, command.BoardId));
        member.Role = command.Role;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}