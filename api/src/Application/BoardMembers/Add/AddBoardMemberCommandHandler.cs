using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Application.BoardMembers.Add;

internal sealed class AddBoardMemberCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<AddBoardMemberCommand>
{
    public async Task<Result> Handle(AddBoardMemberCommand command, CancellationToken cancellationToken = default)
    {
        var member = new BoardMember
        {
            UserId = command.UserId,
            BoardId = command.BoardId,
            Role = command.Role
        };
        await dbContext.BoardMembers.AddAsync(member, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}