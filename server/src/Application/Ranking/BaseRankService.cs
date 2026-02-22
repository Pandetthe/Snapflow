using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Ranking;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal abstract class BaseRankService<TEntity>(
    ILogger logger,
    IAppDbContext dbContext,
    IRankService rankService) : IEntityRankService<TEntity>
    where TEntity : class, IEntity, IRankable
{
    protected ILogger Logger => logger;
    protected IAppDbContext DbContext => dbContext;
    protected IRankService RankService => rankService;

    protected abstract DbSet<TEntity> Entities { get; }
    protected abstract Expression<Func<TEntity, bool>> GroupFilter(int groupId);
    protected abstract Error GetNotFoundError(int id);

    public async Task<Result<string>> GenerateRankAsync(int groupId, int? movingId, int? beforeId,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = Entities
            .AsNoTracking()
            .Where(GroupFilter(groupId))
            .Where(s => !s.IsDeleted && (movingId == null || s.Id != movingId));

        if (!await baseQuery.AnyAsync(cancellationToken))
            return RankService.GenerateInitial();

        string? leftRank = null;
        string? rightRank = null;

        if (beforeId.HasValue)
        {
            rightRank = await baseQuery
                .Where(s => s.Id == beforeId.Value)
                .Select(s => s.Rank)
                .FirstOrDefaultAsync(cancellationToken);
            if (rightRank == null)
                return Result.Failure<string>(GetNotFoundError(beforeId.Value));

            leftRank = await baseQuery
                .Where(s => s.Rank.CompareTo(rightRank) < 0)
                .OrderByDescending(s => s.Rank)
                .Select(s => s.Rank)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            rightRank = null;
            leftRank = await baseQuery
                .OrderByDescending(s => s.Rank)
                .Select(s => s.Rank)
                .FirstOrDefaultAsync(cancellationToken);
            if (leftRank == null)
                return RankService.GenerateInitial();
        }

        if (RankService.TryGenerateBetween(leftRank, rightRank, out var between))
            return between;

        Result result = await NormalizeLocallyAsync(groupId, leftRank, rightRank, cancellationToken);

        if (result.IsSuccess && RankService.TryGenerateBetween(leftRank, rightRank, out between))
            return between;

        result = await NormalizeGloballyAsync(groupId, cancellationToken);

        if (result.IsSuccess && RankService.TryGenerateBetween(leftRank, rightRank, out between))
            return between;

        return Result.Failure<string>(result.Error);
    }

    private sealed record EntityRankDto(int Id, string Rank);

    public async Task<Result> NormalizeLocallyAsync(int groupId, string? leftRank, string? rightRank, CancellationToken cancellationToken = default)
    {
        if (leftRank == null && rightRank == null)
            return Result.Failure(RankingErrors.InvalidNormalizationRange);
            
        await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<EntityRankDto> left = [];
            List<EntityRankDto> right = [];
            
            if (leftRank != null)
            {
                left = await Entities
                    .AsNoTracking()
                    .Where(GroupFilter(groupId))
                    .Where(s => s.Rank.CompareTo(leftRank) < 0 && !s.IsDeleted)
                    .OrderByDescending(s => s.Rank)
                    .Select(s => new EntityRankDto(s.Id, s.Rank))
                    .Take(20)
                    .ToListAsync(cancellationToken);
            }
            
            if (rightRank != null)
            {
                right = await Entities
                    .AsNoTracking()
                    .Where(GroupFilter(groupId))
                    .Where(s => s.Rank.CompareTo(rightRank) > 0 && !s.IsDeleted)
                    .OrderBy(s => s.Rank)
                    .Select(s => new EntityRankDto(s.Id, s.Rank))
                    .Take(20)
                    .ToListAsync(cancellationToken);
            }
            
            List<EntityRankDto> middle = await Entities
                .AsNoTracking()
                .Where(GroupFilter(groupId))
                .Where(s => !s.IsDeleted)
                .Where(s => (leftRank == null || s.Rank.CompareTo(leftRank) >= 0) && (rightRank == null || s.Rank.CompareTo(rightRank) <= 0))
                .OrderBy(s => s.Rank)
                .Select(s => new EntityRankDto(s.Id, s.Rank))
                .ToListAsync(cancellationToken);
                
            var items = left.Concat(middle).Concat(right).OrderBy(s => s.Rank).ToList();
            if (items.Count < 2)
            {
                await tx.CommitAsync(cancellationToken);
                return Result.Success();
            }
            
            var newRanks = RankService.GenerateBalancedBetween(items.Count, items.First().Rank, items.Last().Rank);
            foreach (var (item, newRank) in items.Zip(newRanks))
            {
                await Entities
                    .Where(s => s.Id == item.Id && s.Rank == item.Rank)
                    .ExecuteUpdateAsync(s => s.SetProperty(i => i.Rank, newRank), cancellationToken);
            }
            
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to normalize locally {EntityName} ranks.", typeof(TEntity).Name);
            await tx.RollbackAsync(cancellationToken);
            return Result.Failure(RankingErrors.RankExhausted);
        }
    }

    public async Task<Result> NormalizeGloballyAsync(int? groupId, CancellationToken cancellationToken = default)
    {
        var tableName = Entities.EntityType.GetTableName();
        var schema = Entities.EntityType.GetSchema();
        var fullTableName = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";

        await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
            await DbContext.Database.ExecuteSqlRawAsync(
                $"LOCK TABLE {fullTableName} IN ACCESS EXCLUSIVE MODE;", cancellationToken);
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.

            if (groupId.HasValue)
            {
                var items = await Entities
                    .AsNoTracking()
                    .OrderBy(s => s.Rank)
                    .Where(GroupFilter(groupId.Value))
                    .Where(s => !s.IsDeleted)
                    .Select(s => s.Id)
                    .ToListAsync(cancellationToken);
                    
                if (items.Count == 0)
                    return Result.Success();
                    
                List<string> ranks = RankService.GenerateBalanced(items.Count);

                foreach (var (id, rank) in items.Zip(ranks))
                    await Entities
                        .Where(s => s.Id == id)
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.Rank, rank), cancellationToken);
            }
            else
            {
                await NormalizeAllGroupsAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to normalize {EntityName} ranks.", typeof(TEntity).Name);
            await tx.RollbackAsync(cancellationToken);
            return Result.Failure(RankingErrors.RankExhausted);
        }
    }
    
    protected abstract Task NormalizeAllGroupsAsync(CancellationToken cancellationToken);
}
