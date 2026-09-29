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
        string? userName = await dbContext.FindUserNameAsync(userContext.UserId, cancellationToken);
        if (userName == null)
            return UserErrors.NotFound(userContext.UserId);

        Board? board = await dbContext.Boards
            .Include(b => b.Tags)
            .SingleOrDefaultAsync(b => b.Id == command.BoardId, cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(command.BoardId);

        DateTimeOffset createdAt = timeProvider.GetUtcNow();

        // Titles are what people pick tags by, so the board may not hold two of the same.
        Result<Tag> created = board.CreateTag(
            command.Title,
            command.Color,
            userContext.UserId,
            createdAt,
            userContext.ConnectionId);
        if (created.IsFailure)
            return created.Error;

        Result saved = await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.TagTitle, TagErrors.TitleNotUnique(command.Title))],
            cancellationToken);
        if (saved.IsFailure)
            return saved.Error;

        return new CreateTagResponse(created.Value.Id, createdAt, new UserDto(userContext.UserId, userName));
    }
}
