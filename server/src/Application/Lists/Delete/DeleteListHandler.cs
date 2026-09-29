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
    public Task<Result> Handle(DeleteListCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result> ExecuteAsync(DeleteListCommand command, CancellationToken cancellationToken)
    {
        List? list = await dbContext.Lists
            .SingleOrDefaultAsync(l => l.Id == command.Id && l.BoardId == command.BoardId, cancellationToken);
        if (list == null)
            return ListErrors.NotFound(command.Id);

        DateTimeOffset deletedAt = timeProvider.GetUtcNow();
        list.SoftDelete(userContext.UserId, deletedAt, userContext.ConnectionId);
        await dbContext.CascadeListDeletionAsync(list.Id, userContext.UserId, deletedAt, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
