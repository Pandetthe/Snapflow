using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

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
        Swimlane? swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return SwimlaneErrors.NotFound(command.Id);

        DateTimeOffset deletedAt = timeProvider.GetUtcNow();
        swimlane.SoftDelete(userContext.UserId, deletedAt, userContext.ConnectionId);
        await dbContext.CascadeSwimlaneDeletionAsync(swimlane.Id, userContext.UserId, deletedAt, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
