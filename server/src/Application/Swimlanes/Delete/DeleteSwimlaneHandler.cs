using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Delete;

internal sealed class DeleteSwimlaneHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteSwimlaneCommand>
{
    public Task<Result> Handle(DeleteSwimlaneCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result> ExecuteAsync(DeleteSwimlaneCommand command, CancellationToken cancellationToken)
    {
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return UserErrors.NotFound(userContext.UserId);

        Swimlane? swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return SwimlaneErrors.NotFound(command.Id);

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        var userId = userContext.UserId;

        swimlane.SoftDelete(user, dateTimeOffset, userContext.ConnectionId);

        await dbContext.Lists
            .Where(l => l.SwimlaneId == swimlane.Id)
            .ExecuteUpdateAsync(l => l
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, dateTimeOffset)
                .SetProperty(x => x.DeletedById, userId)
                .SetProperty(x => x.DeletedByCascade, true),
                cancellationToken);

        await dbContext.Cards
            .Where(c => c.SwimlaneId == swimlane.Id)
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