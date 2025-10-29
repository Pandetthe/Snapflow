using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.BoardMembers;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Create;

internal sealed class CreateSwimlaneCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<CreateSwimlaneCommand, int>
{
    public async Task<Result<int>> Handle(CreateSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));
        var swimlane = new Swimlane
        {
            BoardId = command.BoardId,
            Title = command.Title,
            CreatedById = user.Id,
            CreatedAt = timeProvider.GetUtcNow(),
        };
        await dbContext.Swimlanes.AddAsync(swimlane, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return swimlane.Id;
    }
}