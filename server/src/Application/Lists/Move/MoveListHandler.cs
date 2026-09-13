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
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<string>(UserErrors.NotFound(userContext.UserId));

        Swimlane? swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == command.SwimlaneId && s.BoardId == command.BoardId && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure<string>(SwimlaneErrors.NotFound(command.SwimlaneId));

        List? list = await dbContext.Lists
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId && !s.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure<string>(ListErrors.NotFound(command.Id));

        var rankResult = await rankService.GenerateRankAsync(
            command.SwimlaneId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<string>(rankResult.Error);

        list.Move(
            command.SwimlaneId,
            rankResult.Value,
            user,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(rankResult.Value);
    }
}