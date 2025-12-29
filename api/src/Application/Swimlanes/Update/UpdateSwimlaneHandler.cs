using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Update;

internal sealed class UpdateSwimlaneHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateSwimlaneCommand>
{
    public async Task<Result> Handle(UpdateSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        var swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure(SwimlaneErrors.NotFound(command.Id));

        swimlane.Title = command.Title;
        swimlane.Height = command.Height;
        swimlane.UpdatedById = userContext.UserId;
        swimlane.UpdatedAt = timeProvider.GetUtcNow();

        swimlane.Raise(new SwimlaneUpdatedDomainEvent(swimlane.Id, swimlane.BoardId, swimlane.Title));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}