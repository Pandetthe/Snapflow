using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Ranking;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Ranking;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal sealed class CardRankService(
    IAppDbContext dbContext,
    IRankService rankService)
    : BaseRankService<Card>(dbContext, rankService)
{
    protected override DbSet<Card> Entities => DbContext.Cards;

    protected override Expression<Func<Card, int>> GroupKey => s => s.ListId;

    protected override Expression<Func<Card, bool>> GroupFilter(int groupId) =>
        s => s.ListId == groupId;

    protected override Error GetNotFoundError(int id) =>
        CardErrors.NotFound(id);
}
