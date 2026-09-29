using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;
using static Snapflow.Application.Swimlanes.Update.UpdateSwimlaneResponse;

namespace Snapflow.Application.Swimlanes.Update;

internal sealed class UpdateSwimlaneHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateSwimlaneCommand, UpdateSwimlaneResponse>
{
    public async Task<Result<UpdateSwimlaneResponse>> Handle(UpdateSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        string? userName = await dbContext.FindUserNameAsync(userContext.UserId, cancellationToken);
        if (userName == null)
            return UserErrors.NotFound(userContext.UserId);

        Swimlane? swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return SwimlaneErrors.NotFound(command.Id);

        DateTimeOffset updatedAt = timeProvider.GetUtcNow();

        bool changed = swimlane.Update(
            command.Title,
            command.Height,
            userContext.UserId,
            updatedAt,
            userContext.ConnectionId);

        if (!changed)
            return new UpdateSwimlaneResponse(null, null);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateSwimlaneResponse(
            updatedAt,
            new UserDto(userContext.UserId, userName));
    }
}