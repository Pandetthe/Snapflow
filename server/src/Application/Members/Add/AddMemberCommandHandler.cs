using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Add;

internal sealed class AddMemberCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<AddMemberCommand>
{
    public async Task<Result> Handle(AddMemberCommand command, CancellationToken cancellationToken = default)
    {
        var existingOwner = await dbContext.Members
            .AsNoTracking()
            .AnyAsync(m => m.BoardId == command.BoardId && m.Role == MemberRole.Owner, cancellationToken);
        
        if (existingOwner && command.Role == MemberRole.Owner)
            return Result.Failure(MemberErrors.OwnerAlreadyExists(command.BoardId));

        var member = Member.Create(command.BoardId, command.UserId, command.Role);

        await dbContext.Members.AddAsync(member, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}