using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Ranking;
using Snapflow.Domain.Swimlanes;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal sealed class SwimlaneRankService(
    ILogger<SwimlaneRankService> logger,
    IAppDbContext dbContext,
    IRankService rankService) 
    : BaseRankService<Swimlane>(logger, dbContext, rankService)
{
    protected override DbSet<Swimlane> Entities => DbContext.Swimlanes;

    protected override Expression<Func<Swimlane, int>> GroupKey => s => s.BoardId;

    protected override Expression<Func<Swimlane, bool>> GroupFilter(int groupId) => 
        s => s.BoardId == groupId;

    protected override Error GetNotFoundError(int id) => 
        SwimlaneErrors.NotFound(id);
}
