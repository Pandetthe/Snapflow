using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Remove;

internal sealed class RemoveMemberCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<RemoveMemberCommand>
{
    public async Task<Result> Handle(RemoveMemberCommand command, CancellationToken cancellationToken = default)
    {
        var member = await dbContext.Members
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId
            && b.UserId == command.UserId, cancellationToken);
        if (member == null)
            return Result.Failure(MemberErrors.NotFound(command.UserId, command.BoardId));
        member.Remove(userContext.ConnectionId);
        dbContext.Members.Remove(member);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}