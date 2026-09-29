using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Members.Add;

internal sealed class AddMemberCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<AddMemberCommand>
{
    public async Task<Result> Handle(AddMemberCommand command, CancellationToken cancellationToken = default)
    {
        var boardExists = await dbContext.Boards
            .AsNoTracking()
            .AnyAsync(b => b.Id == command.BoardId && !b.IsDeleted, cancellationToken);
        if (!boardExists)
            return Result.Failure(BoardErrors.NotFound(command.BoardId));

        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == command.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var existingOwner = await dbContext.Members
            .AsNoTracking()
            .AnyAsync(m => m.BoardId == command.BoardId && m.Role == MemberRole.Owner, cancellationToken);

        if (existingOwner && command.Role == MemberRole.Owner)
            return Result.Failure(MemberErrors.OwnerAlreadyExists(command.BoardId));

        var alreadyMember = await dbContext.Members
            .AsNoTracking()
            .AnyAsync(m => m.BoardId == command.BoardId && m.UserId == command.UserId, cancellationToken);
        if (alreadyMember)
            return Result.Failure(MemberErrors.AlreadyMember(command.UserId, command.BoardId));

        var member = Member.Create(command.BoardId, command.UserId, command.Role);

        await dbContext.Members.AddAsync(member, cancellationToken);

        return await dbContext.TrySaveChangesAsync(
            [
                new UniqueConflict(DbConstraints.BoardMemberKey, MemberErrors.AlreadyMember(command.UserId, command.BoardId)),
                new UniqueConflict(DbConstraints.BoardSingleOwner, MemberErrors.OwnerAlreadyExists(command.BoardId))
            ],
            cancellationToken);
    }
}
