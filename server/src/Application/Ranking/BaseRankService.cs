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
        long globalLockKey = GetGlobalLockKey();
        long groupLockKey = GetGroupLockKey(groupId);
        
        var strategy = DbContext.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () => 
        {
            await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            
            try 
            {
                await DbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})", 
                    [globalLockKey, groupLockKey], cancellationToken);

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
                {
                    await tx.CommitAsync(cancellationToken);
                    return between;
                }

                Result result = await NormalizeLocallyAsync(groupId, leftRank, rightRank, cancellationToken);

                if (result.IsSuccess && RankService.TryGenerateBetween(leftRank, rightRank, out between))
                {
                    await tx.CommitAsync(cancellationToken);
                    return between;
                }

                result = await NormalizeGloballyAsync(groupId, cancellationToken);

                if (result.IsSuccess && RankService.TryGenerateBetween(leftRank, rightRank, out between))
                {
                    await tx.CommitAsync(cancellationToken);
                    return between;
                }

                await tx.RollbackAsync(cancellationToken);
                return Result.Failure<string>(result.Error);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to generate rank for {EntityName} in group {GroupId}", typeof(TEntity).Name, groupId);
                await tx.RollbackAsync(cancellationToken);
                return Result.Failure<string>(RankingErrors.RankExhausted);
            }
        });
    }

    private sealed record EntityRankDto(int Id, string Rank);

    public async Task<Result> NormalizeLocallyAsync(int groupId, string? leftRank, string? rightRank, CancellationToken cancellationToken = default)
    {
        if (leftRank == null && rightRank == null)
            return Result.Failure(RankingErrors.InvalidNormalizationRange);

        var strategy = DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                long globalLockKey = GetGlobalLockKey();
                long groupLockKey = GetGroupLockKey(groupId);
                await DbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})",
                    [globalLockKey, groupLockKey], cancellationToken);

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

                var updateTasks = items.Zip(newRanks).Select(pair =>
                    Entities
                        .Where(s => s.Id == pair.First.Id && s.Rank == pair.First.Rank)
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.Rank, pair.Second), cancellationToken)
                );

                await Task.WhenAll(updateTasks);

                await tx.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to normalize locally {EntityName} ranks.", typeof(TEntity).Name);
                await tx.RollbackAsync(cancellationToken);
                return Result.Failure(RankingErrors.RankExhausted);
            }
        });
    }

    public async Task<Result> NormalizeGloballyAsync(int? groupId, CancellationToken cancellationToken = default)
    {
        var tableName = Entities.EntityType.GetTableName();
        var schema = Entities.EntityType.GetSchema();
        var fullTableName = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";

        var strategy = DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (groupId.HasValue)
                {
                    long globalLockKey = GetGlobalLockKey();
                    long groupLockKey = GetGroupLockKey(groupId.Value);
                    await DbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})",
                        [globalLockKey, groupLockKey], cancellationToken);

                    var items = await Entities
                        .AsNoTracking()
                        .OrderBy(s => s.Rank)
                        .Where(GroupFilter(groupId.Value))
                        .Where(s => !s.IsDeleted)
                        .Select(s => s.Id)
                        .ToListAsync(cancellationToken);

                    if (items.Count == 0)
                    {
                        await tx.CommitAsync(cancellationToken);
                        return Result.Success();
                    }

                    List<string> ranks = RankService.GenerateBalanced(items.Count);

                    var updateTasks = items.Zip(ranks).Select(pair =>
                        Entities
                            .Where(s => s.Id == pair.First)
                            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Rank, pair.Second), cancellationToken)
                    );

                    await Task.WhenAll(updateTasks);
                }
                else
                {
                    long globalLockKey = GetGlobalLockKey();
                    await DbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock({0})", [globalLockKey], cancellationToken);

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
        });
    }

    protected abstract Task NormalizeAllGroupsAsync(CancellationToken cancellationToken);

    private static long GetGlobalLockKey()
    {
        return (long)typeof(TEntity).Name.GetHashCode() << 32;
    }

    private static long GetGroupLockKey(int groupId)
    {
        return ((long)typeof(TEntity).Name.GetHashCode() << 32) | (uint)groupId;
    }
}
