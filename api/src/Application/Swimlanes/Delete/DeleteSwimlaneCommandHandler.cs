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
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));
        var swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id 
            && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return Result.Failure(SwimlaneErrors.NotFound(command.Id));
        swimlane.IsDeleted = true;
        swimlane.DeletedById = user.Id;
        swimlane.DeletedAt = timeProvider.GetUtcNow();
        swimlane.Raise(new SwimlaneDeletedDomainEvent(swimlane.Id, swimlane.BoardId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}