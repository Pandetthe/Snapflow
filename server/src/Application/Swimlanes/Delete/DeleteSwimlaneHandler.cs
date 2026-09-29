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
    public async Task<Result> Handle(DeleteSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        Swimlane? swimlane = await dbContext.Swimlanes
            .Include(s => s.Lists)
            .Include(s => s.Cards)
            .AsSplitQuery()
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return SwimlaneErrors.NotFound(command.Id);

        swimlane.SoftDelete(userContext.UserId, timeProvider.GetUtcNow(), userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
