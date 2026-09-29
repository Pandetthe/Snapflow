using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Tags.Delete;

internal sealed class DeleteTagHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteTagCommand>
{
    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken = default)
    {
        // The cards keep their join rows: the tag is only soft deleted, and every read filters it out.
        Board? board = await dbContext.Boards
            .Include(b => b.Tags)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board == null)
            return TagErrors.NotFound(command.Id);

        Result deleted = board.DeleteTag(command.Id, userContext.UserId, timeProvider.GetUtcNow(), userContext.ConnectionId);
        if (deleted.IsFailure)
            return deleted;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
