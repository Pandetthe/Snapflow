using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;
using static Snapflow.Application.Tags.Create.CreateTagResponse;

namespace Snapflow.Application.Tags.Create;

internal sealed class CreateTagHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<CreateTagCommand, CreateTagResponse>
{
    public async Task<Result<CreateTagResponse>> Handle(CreateTagCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<CreateTagResponse>(UserErrors.NotFound(userContext.UserId));

        var boardExists = await dbContext.Boards.AsNoTracking()
            .AnyAsync(b => b.Id == command.BoardId && !b.IsDeleted, cancellationToken);
        if (!boardExists)
            return Result.Failure<CreateTagResponse>(BoardErrors.NotFound(command.BoardId));

        // Titles are what people pick tags by, so the board may not hold two of the same.
        var titleTaken = await dbContext.Tags.AsNoTracking()
            .AnyAsync(t => t.BoardId == command.BoardId && !t.IsDeleted && t.Title == command.Title, cancellationToken);
        if (titleTaken)
            return Result.Failure<CreateTagResponse>(TagErrors.TitleNotUnique(command.Title));

        DateTimeOffset createdAt = timeProvider.GetUtcNow();

        var tag = Tag.Create(
            command.BoardId,
            command.Title,
            command.Color,
            user,
            createdAt,
            userContext.ConnectionId);

        await dbContext.Tags.AddAsync(tag, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTagResponse(tag.Id, createdAt, UserDto.From(user));
    }
}
