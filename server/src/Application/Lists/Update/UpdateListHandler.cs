using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Update;

internal sealed class UpdateListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateListCommand>
{
    public async Task<Result> Handle(UpdateListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .SingleOrDefaultAsync(l => l.Id == command.Id && !l.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));

        list.Title = command.Title;
        list.Width = command.Width;
        list.UpdatedById = userContext.UserId;
        list.UpdatedAt = timeProvider.GetUtcNow();

        list.Raise((entity) =>
            new ListUpdatedDomainEvent(entity.Id, entity.BoardId, entity.Title,
                entity.Width, userContext.ConnectionId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}