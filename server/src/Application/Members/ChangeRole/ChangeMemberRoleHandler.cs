using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.ChangeRole;

internal sealed class ChangeMemberRoleHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<ChangeMemberRoleCommand>
{
    public async Task<Result> Handle(ChangeMemberRoleCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board is null)
            return BoardErrors.NotFound(command.BoardId);

        Result changed = board.ChangeMemberRole(command.UserId, command.Role, userContext.ConnectionId);
        if (changed.IsFailure)
            return changed;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}