using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Ranking;
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
    public Task<Result<string>> Handle(MoveListCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result<string>> ExecuteAsync(MoveListCommand command, CancellationToken cancellationToken)
    {
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return UserErrors.NotFound(userContext.UserId);

        Swimlane? swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == command.SwimlaneId && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return SwimlaneErrors.NotFound(command.SwimlaneId);

        List? list = await dbContext.Lists
            .Include(l => l.Cards)
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId, cancellationToken);
        if (list == null)
            return ListErrors.NotFound(command.Id);

        var rankResult = await rankService.GenerateRankAsync(
            command.SwimlaneId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return rankResult.Error;

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