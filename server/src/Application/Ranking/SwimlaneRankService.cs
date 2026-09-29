using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Ranking;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Ranking;
using Snapflow.Domain.Swimlanes;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal sealed class SwimlaneRankService(
    IAppDbContext dbContext,
    IRankService rankService)
    : BaseRankService<Swimlane>(dbContext, rankService)
{
    protected override DbSet<Swimlane> Entities => DbContext.Swimlanes;

    protected override Expression<Func<Swimlane, int>> GroupKey => s => s.BoardId;

    protected override Expression<Func<Swimlane, bool>> GroupFilter(int groupId) => 
        s => s.BoardId == groupId;

    protected override Error GetNotFoundError(int id) => 
        SwimlaneErrors.NotFound(id);
}
