using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Delete;

internal sealed class DeleteListCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteListCommand>
{
    public async Task<Result> Handle(DeleteListCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));
        var list = await dbContext.Lists
            .SingleOrDefaultAsync(s => s.Id == command.Id
            && s.BoardId == command.BoardId
            && s.SwimlaneId == command.SwimlaneId, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));
        list.IsDeleted = true;
        list.DeletedById = user.Id;
        list.DeletedAt = timeProvider.GetUtcNow();
        list.Raise(new ListDeletedDomainEvent(list.Id, list.BoardId, list.SwimlaneId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}