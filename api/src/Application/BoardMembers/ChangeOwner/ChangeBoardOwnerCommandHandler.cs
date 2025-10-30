using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.BoardMembers;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.BoardMembers.ChangeOwner;

internal sealed class ChangeBoardOwnerCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<ChangeBoardOwnerCommand>
{
    public async Task<Result> Handle(ChangeBoardOwnerCommand command, CancellationToken cancellationToken = default)
    {
        var boardOwner = await dbContext.BoardMembers
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.Role == BoardMemberRole.Owner, cancellationToken);
        if (boardOwner == null)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));
        var newOwner = await dbContext.BoardMembers
            .SingleOrDefaultAsync(b => b.BoardId == command.BoardId && b.UserId == command.UserId, cancellationToken);
        if (newOwner == null)
            return Result.Failure(BoardMemberErrors.NotFound(command.UserId, command.BoardId));
        boardOwner.Role = BoardMemberRole.Admin;
        newOwner.Role = BoardMemberRole.Owner;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}