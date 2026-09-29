using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Lists.Delete;

internal sealed class DeleteListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteListCommand>
{
    public async Task<Result> Handle(DeleteListCommand command, CancellationToken cancellationToken = default)
    {
        List? list = await dbContext.Lists
            .Include(l => l.Cards)
            .SingleOrDefaultAsync(l => l.Id == command.Id && l.BoardId == command.BoardId, cancellationToken);
        if (list == null)
            return ListErrors.NotFound(command.Id);

        list.SoftDelete(userContext.UserId, timeProvider.GetUtcNow(), userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
