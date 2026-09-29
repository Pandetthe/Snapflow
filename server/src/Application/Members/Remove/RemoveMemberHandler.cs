using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Remove;

internal sealed class RemoveMemberHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<RemoveMemberCommand>
{
    public async Task<Result> Handle(RemoveMemberCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board is null)
            return BoardErrors.NotFound(command.BoardId);

        Result removed = board.RemoveMember(command.UserId, userContext.ConnectionId);
        if (removed.IsFailure)
            return removed;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}