using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Move;

internal sealed class MoveListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<List> rankService) : ICommandHandler<MoveListCommand, string>
{
    public async Task<Result<string>> Handle(MoveListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<string>(UserErrors.NotFound(userContext.UserId));

        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == command.SwimlaneId && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure<string>(SwimlaneErrors.NotFound(command.Id));

        var list = await dbContext.Lists
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure<string>(ListErrors.NotFound(command.Id));

        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.SwimlaneId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<string>(rankResult.Error);

        list.Move(
            command.SwimlaneId,
            rankResult.Value,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(rankResult.Value);
    }
}