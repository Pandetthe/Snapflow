using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
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
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UpdateTagResponse>(UserErrors.NotFound(userContext.UserId));

        Tag? tag = await dbContext.Tags
            .SingleOrDefaultAsync(t => t.Id == command.Id && t.BoardId == command.BoardId && !t.IsDeleted, cancellationToken);
        if (tag == null)
            return Result.Failure<UpdateTagResponse>(TagErrors.NotFound(command.Id));

        var titleTaken = await dbContext.Tags.AsNoTracking()
            .AnyAsync(t => t.BoardId == command.BoardId && t.Id != command.Id && !t.IsDeleted && t.Title == command.Title, cancellationToken);
        if (titleTaken)
            return Result.Failure<UpdateTagResponse>(TagErrors.TitleNotUnique(command.Title));

        DateTimeOffset updatedAt = timeProvider.GetUtcNow();

        tag.Update(command.Title, command.Color, user, updatedAt, userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateTagResponse(updatedAt, UserDto.From(user));
    }
}
