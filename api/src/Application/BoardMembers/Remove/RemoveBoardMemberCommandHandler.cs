using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Application.BoardMembers.Remove;

internal sealed class RemoveBoardMemberCommandHandler(
    IAppDbContext dbContext) : ICommandHandler<RemoveBoardMemberCommand>
{
    public async Task<Result> Handle(RemoveBoardMemberCommand command, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.BoardMembers
            .Where(b => b.BoardId == command.BoardId && b.UserId == command.UserId)
            .ExecuteDeleteAsync(cancellationToken);
        if (result == 0)
            return Result.Failure(BoardMemberErrors.NotFound(command.UserId, command.BoardId));
        return Result.Success();
    }
}