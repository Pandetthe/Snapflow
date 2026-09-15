using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
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
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        // The cards keep their join rows: the tag is only soft deleted, and every read filters it out.
        Tag? tag = await dbContext.Tags
            .SingleOrDefaultAsync(t => t.Id == command.Id && t.BoardId == command.BoardId && !t.IsDeleted, cancellationToken);
        if (tag == null)
            return Result.Failure(TagErrors.NotFound(command.Id));

        tag.SoftDelete(user, timeProvider.GetUtcNow(), userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
