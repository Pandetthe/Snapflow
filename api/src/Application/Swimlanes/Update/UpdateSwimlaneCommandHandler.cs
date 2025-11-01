using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Update;

internal sealed class UpdateSwimlaneCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateSwimlaneCommand>
{
    public async Task<Result> Handle(UpdateSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        var swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
        if (swimlane == null)
            return Result.Failure(SwimlaneErrors.NotFound(command.Id));
        swimlane.Title = command.Title;
        swimlane.UpdatedById = user.Id;
        swimlane.UpdatedAt = timeProvider.GetUtcNow();
        swimlane.Raise(new SwimlaneUpdatedDomainEvent(swimlane.Id, swimlane.BoardId, swimlane.Title));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}