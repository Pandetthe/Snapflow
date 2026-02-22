using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Ranking;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal sealed class ListRankService(
    ILogger<ListRankService> logger,
    IAppDbContext dbContext,
    IRankService rankService) 
    : BaseRankService<List>(logger, dbContext, rankService)
{
    protected override DbSet<List> Entities => DbContext.Lists;

    protected override Expression<Func<List, bool>> GroupFilter(int groupId) => 
        s => s.SwimlaneId == groupId;

    protected override Error GetNotFoundError(int id) => 
        ListErrors.NotFound(id);

    protected override async Task NormalizeAllGroupsAsync(CancellationToken cancellationToken)
    {
        var swimlaneIds = await DbContext.Lists
            .Where(s => !s.IsDeleted)
            .Select(s => s.SwimlaneId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var bId in swimlaneIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lists = await DbContext.Lists
                .OrderBy(s => s.Rank)
                .Where(s => s.SwimlaneId == bId && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
            if (lists.Count == 0)
                continue;
                
            List<string> ranks = RankService.GenerateBalanced(lists.Count);

            foreach (var (listId, rank) in lists.Zip(ranks))
                await DbContext.Lists
                    .Where(s => s.Id == listId)
                    .ExecuteUpdateAsync(s => s.SetProperty(sl => sl.Rank, rank), cancellationToken);
        }
    }
}
