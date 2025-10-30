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
        var member = new Member
        {
            UserId = command.UserId,
            BoardId = command.BoardId,
            Role = command.Role
        };
        await dbContext.Members.AddAsync(member, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}