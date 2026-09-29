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
        Swimlane? swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == command.SwimlaneId && s.BoardId == command.BoardId, cancellationToken);
        if (swimlane == null)
            return SwimlaneErrors.NotFound(command.SwimlaneId);

        List? list = await dbContext.Lists
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
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.Cards
            .IgnoreQueryFilters([ISoftDeletable.FilterName])
            .Where(c => c.ListId == list.Id && c.SwimlaneId != command.SwimlaneId)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.SwimlaneId, command.SwimlaneId), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(rankResult.Value);
    }
}