using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using static Snapflow.Application.Tags.Update.UpdateTagResponse;

namespace Snapflow.Application.Tags.Update;

internal sealed class UpdateTagHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateTagCommand, UpdateTagResponse>
{
    public async Task<Result<UpdateTagResponse>> Handle(UpdateTagCommand command, CancellationToken cancellationToken = default)
    {
        string? userName = await dbContext.FindUserNameAsync(userContext.UserId, cancellationToken);
        if (userName == null)
            return UserErrors.NotFound(userContext.UserId);

        Board? board = await dbContext.Boards
            .Include(b => b.Tags)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board == null)
            return TagErrors.NotFound(command.Id);

        DateTimeOffset updatedAt = timeProvider.GetUtcNow();

        Result<bool> changed = board.UpdateTag(
            command.Id, command.Title, command.Color, userContext.UserId, updatedAt, userContext.ConnectionId);
        if (changed.IsFailure)
            return changed.Error;
        if (!changed.Value)
            return new UpdateTagResponse(null, null);

        Result saved = await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.TagTitle, TagErrors.TitleNotUnique(command.Title))],
            cancellationToken);
        if (saved.IsFailure)
            return saved.Error;

        return new UpdateTagResponse(updatedAt, new UserDto(userContext.UserId, userName));
    }
}
