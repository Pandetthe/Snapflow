using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

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
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        List? list = await dbContext.Lists
            .SingleOrDefaultAsync(l => l.Id == command.Id && l.BoardId == command.BoardId && !l.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        var userId = userContext.UserId;

        list.SoftDelete(user, dateTimeOffset, userContext.ConnectionId);

        await dbContext.Cards
            .Where(c => c.ListId == list.Id && !c.IsDeleted)
            .ExecuteUpdateAsync(c => c
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, dateTimeOffset)
                .SetProperty(x => x.DeletedById, userId)
                .SetProperty(x => x.DeletedByCascade, true),
                cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}