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
    public async Task<Result> Handle(DeleteSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        var swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure(SwimlaneErrors.NotFound(command.Id));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            swimlane.IsDeleted = true;
            swimlane.DeletedById = userId;
            swimlane.DeletedAt = dateTimeOffset;
            swimlane.DeletedByCascade = false;

            await dbContext.Lists
                .Where(l => l.SwimlaneId == swimlane.Id && !l.IsDeleted)
                .ExecuteUpdateAsync(l => l
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId)
                    .SetProperty(x => x.DeletedByCascade, true),
                    cancellationToken);

            await dbContext.Cards
                .Where(c => c.SwimlaneId == swimlane.Id && !c.IsDeleted)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId)
                    .SetProperty(x => x.DeletedByCascade, true),
                    cancellationToken);

            swimlane.Raise((entity) =>
                new SwimlaneDeletedDomainEvent(entity.Id, entity.BoardId, userContext.ConnectionId));

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Result.Success();
    }
}