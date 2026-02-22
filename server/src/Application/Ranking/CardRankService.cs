using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Ranking;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal sealed class CardRankService(
    ILogger<CardRankService> logger,
    IAppDbContext dbContext,
    IRankService rankService) 
    : BaseRankService<Card>(logger, dbContext, rankService)
{
    protected override DbSet<Card> Entities => DbContext.Cards;

    protected override Expression<Func<Card, bool>> GroupFilter(int groupId) => 
        s => s.ListId == groupId;

    protected override Error GetNotFoundError(int id) => 
        CardErrors.NotFound(id);

    protected override async Task NormalizeAllGroupsAsync(CancellationToken cancellationToken)
    {
        var listIds = await DbContext.Cards
            .Where(s => !s.IsDeleted)
            .Select(s => s.ListId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var bId in listIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cards = await DbContext.Cards
                .OrderBy(s => s.Rank)
                .Where(s => s.ListId == bId && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
            if (cards.Count == 0)
                continue;
                
            List<string> ranks = RankService.GenerateBalanced(cards.Count);

            foreach (var (cardId, rank) in cards.Zip(ranks))
                await DbContext.Cards
                    .Where(s => s.Id == cardId)
                    .ExecuteUpdateAsync(s => s.SetProperty(sl => sl.Rank, rank), cancellationToken);
        }
    }
}
