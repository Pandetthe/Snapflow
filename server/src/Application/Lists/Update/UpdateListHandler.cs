using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;
using static Snapflow.Application.Lists.Update.UpdateListResponse;

namespace Snapflow.Application.Lists.Update;

internal sealed class UpdateListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateListCommand, UpdateListResponse>
{
    public async Task<Result<UpdateListResponse>> Handle(UpdateListCommand command, CancellationToken cancellationToken = default)
    {
        string? userName = await dbContext.FindUserNameAsync(userContext.UserId, cancellationToken);
        if (userName == null)
            return UserErrors.NotFound(userContext.UserId);

        List? list = await dbContext.Lists
            .SingleOrDefaultAsync(l => l.Id == command.Id && l.BoardId == command.BoardId, cancellationToken);
        if (list == null)
            return ListErrors.NotFound(command.Id);

        DateTimeOffset updatedAt = timeProvider.GetUtcNow();

        bool changed = list.Update(
            command.Title,
            command.Width,
            userContext.UserId,
            updatedAt,
            userContext.ConnectionId);

        if (!changed)
            return new UpdateListResponse(null, null);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateListResponse(
            updatedAt,
            new UserDto(userContext.UserId, userName));
    }
}