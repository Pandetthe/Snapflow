using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Ranking;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Ranking;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal sealed class ListRankService(
    IAppDbContext dbContext,
    IRankService rankService)
    : BaseRankService<List>(dbContext, rankService)
{
    protected override DbSet<List> Entities => DbContext.Lists;

    protected override Expression<Func<List, int>> GroupKey => s => s.SwimlaneId;

    protected override Expression<Func<List, bool>> GroupFilter(int groupId) =>
        s => s.SwimlaneId == groupId;

    protected override Error GetNotFoundError(int id) =>
        ListErrors.NotFound(id);
}
