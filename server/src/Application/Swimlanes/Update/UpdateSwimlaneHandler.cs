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
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UpdateSwimlaneResponse>(UserErrors.NotFound(userContext.UserId));

        var swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure<UpdateSwimlaneResponse>(SwimlaneErrors.NotFound(command.Id));

        var updatedAt = timeProvider.GetUtcNow();

        swimlane.Update(
            command.Title,
            command.Height,
            userContext.UserId,
            updatedAt,
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateSwimlaneResponse(
            updatedAt,
            UserDto.From(user));
    }
}