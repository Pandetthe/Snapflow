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
            .Include(s => s.Lists.Where(l => !l.IsDeleted))
            .Include(s => s.Cards.Where(c => !c.IsDeleted))
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure(SwimlaneErrors.NotFound(command.Id));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        swimlane.IsDeleted = true;
        swimlane.DeletedById = userId;
        swimlane.DeletedAt = dateTimeOffset;
        swimlane.DeletedByCascade = false;

        // TODO: Optimize to not load entire lists and cards into memory
        //       Domain events for now cannot be raised without loading and
        //       and modifying tracked entity.

        foreach (var list in swimlane.Lists)
        {
            list.IsDeleted = true;
            list.DeletedById = userId;
            list.DeletedAt = dateTimeOffset;
            list.DeletedByCascade = true;
        }
        foreach (var card in swimlane.Cards)
        {
            card.IsDeleted = true;
            card.DeletedById = userId;
            card.DeletedAt = dateTimeOffset;
            card.DeletedByCascade = true;
        }

        swimlane.Raise((entity) =>
            new SwimlaneDeletedDomainEvent(entity.Id, entity.BoardId, userContext.ConnectionId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}