using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Members.Add;

internal sealed class AddMemberHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<AddMemberCommand>
{
    public async Task<Result> Handle(AddMemberCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .Include(b => b.Members)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board is null)
            return BoardErrors.NotFound(command.BoardId);

        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == command.UserId, cancellationToken);
        if (!userExists)
            return UserErrors.NotFound(command.UserId);

        Result added = board.AddMember(command.UserId, command.Role, userContext.ConnectionId);
        if (added.IsFailure)
            return added;

        return await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.BoardMemberKey, MemberErrors.AlreadyMember(command.UserId, command.BoardId))],
            cancellationToken);
    }
}
