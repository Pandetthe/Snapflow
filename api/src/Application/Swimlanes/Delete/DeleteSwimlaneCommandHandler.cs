using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Delete;

internal sealed class DeleteSwimlaneCommandHandler(
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
            .SingleOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
        if (swimlane == null || swimlane.IsDeleted)
            return Result.Failure(SwimlaneErrors.NotFound(command.Id));

        swimlane.IsDeleted = true;
        swimlane.DeletedById = userContext.UserId;
        swimlane.DeletedAt = timeProvider.GetUtcNow();

        swimlane.Raise(new SwimlaneDeletedDomainEvent(swimlane.Id, swimlane.BoardId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}