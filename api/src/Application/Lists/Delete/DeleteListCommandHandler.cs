using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Delete;

internal sealed class DeleteListCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteListCommand>
{
    public async Task<Result> Handle(DeleteListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .Include(l => l.Cards.Where(c => !c.IsDeleted))
            .SingleOrDefaultAsync(l => l.Id == command.Id && !l.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        list.IsDeleted = true;
        list.DeletedById = userId;
        list.DeletedAt = dateTimeOffset;
        list.DeletedByCascade = false;

        // TODO: Optimize to not load entire cards into memory
        //       Domain events for now cannot be raised without loading and
        //       and modifying tracked entity.

        foreach (var card in list.Cards)
        {
            card.IsDeleted = true;
            card.DeletedById = userId;
            card.DeletedAt = dateTimeOffset;
            card.DeletedByCascade = true;
        }

        list.Raise(new ListDeletedDomainEvent(list.Id, list.BoardId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}