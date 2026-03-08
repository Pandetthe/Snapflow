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
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UpdateListResponse>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .SingleOrDefaultAsync(l => l.Id == command.Id && !l.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure<UpdateListResponse>(ListErrors.NotFound(command.Id));

        var updatedAt = timeProvider.GetUtcNow();

        list.Update(
            command.Title,
            command.Width,
            userContext.UserId,
            updatedAt,
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateListResponse(
            updatedAt,
            UserDto.From(user));
    }
}