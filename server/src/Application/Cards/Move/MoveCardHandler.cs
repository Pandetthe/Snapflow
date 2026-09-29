using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Move;

internal sealed class MoveCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Card> rankService) : ICommandHandler<MoveCardCommand, string>
{
    public Task<Result<string>> Handle(MoveCardCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result<string>> ExecuteAsync(MoveCardCommand command, CancellationToken cancellationToken)
    {
        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(l => l.Id == command.ListId && l.BoardId == command.BoardId)
            .Select(l => new { l.SwimlaneId })
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return ListErrors.NotFound(command.ListId);

        Card? card = await dbContext.Cards
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId, cancellationToken);
        if (card == null)
            return CardErrors.NotFound(command.Id);

        var rankResult = await rankService.GenerateRankAsync(
            command.ListId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return rankResult.Error;

        card.Move(
            command.ListId,
            list.SwimlaneId,
            rankResult.Value,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(rankResult.Value);
    }
}