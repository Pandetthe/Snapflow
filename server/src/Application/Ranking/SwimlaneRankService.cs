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

    protected override Expression<Func<Swimlane, bool>> GroupFilter(int groupId) => 
        s => s.BoardId == groupId;

    protected override Error GetNotFoundError(int id) => 
        SwimlaneErrors.NotFound(id);

    protected override async Task NormalizeAllGroupsAsync(CancellationToken cancellationToken)
    {
        var boardIds = await DbContext.Swimlanes
            .Where(s => !s.IsDeleted)
            .Select(s => s.BoardId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var bId in boardIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var swimlanes = await DbContext.Swimlanes
                .OrderBy(s => s.Rank)
                .Where(s => s.BoardId == bId && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
            if (swimlanes.Count == 0)
                continue;
                
            List<string> ranks = RankService.GenerateBalanced(swimlanes.Count);

            foreach (var (swimlaneId, rank) in swimlanes.Zip(ranks))
                await DbContext.Swimlanes
                    .Where(s => s.Id == swimlaneId)
                    .ExecuteUpdateAsync(s => s.SetProperty(sl => sl.Rank, rank), cancellationToken);
        }
    }
}
