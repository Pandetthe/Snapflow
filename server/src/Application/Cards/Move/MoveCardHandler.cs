using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Move;

internal sealed class MoveCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Card> rankService) : ICommandHandler<MoveCardCommand>
{
    public async Task<Result> Handle(MoveCardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == command.ListId && !l.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));

        var card = await dbContext.Cards
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (card == null)
            return Result.Failure(CardErrors.NotFound(command.Id));
        card.ListId = command.ListId;
        card.SwimlaneId = list.SwimlaneId;
        Result<string> rankResult = await rankService.GenerateRankAsync(
            card.ListId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure(rankResult.Error);
        card.Rank = rankResult.Value;
        card.UpdatedById = userContext.UserId;
        card.UpdatedAt = timeProvider.GetUtcNow();
        card.Raise((entity) =>
            new CardMovedDomainEvent(entity.Id, entity.BoardId, entity.ListId,
                entity.Rank, userContext.ConnectionId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}