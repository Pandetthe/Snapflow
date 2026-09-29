using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Ranking;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

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
    protected abstract Expression<Func<TEntity, int>> GroupKey { get; }
    protected abstract Expression<Func<TEntity, bool>> GroupFilter(int groupId);
    protected abstract Error GetNotFoundError(int id);

    public async Task<Result<string>> GenerateRankAsync(int groupId, int? movingId, int? beforeId,
        CancellationToken cancellationToken = default)
    {
        var globalLockKey = GetGlobalLockKey();
        var groupLockKey = GetGroupLockKey(groupId);

        IExecutionStrategy strategy = DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);

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

                Func<Neighbours, Task<Result>>[] normalizations =
                [
                    n => NormalizeLocallyInternalAsync(groupId, n.Left, n.Right, cancellationToken),
                    _ => NormalizeGroupInternalAsync(groupId, cancellationToken)
                ];

                for (int attempt = 0; ; attempt++)
                {
                    Result<Neighbours> neighbours = await FindNeighboursAsync(baseQuery, beforeId, cancellationToken);
                    if (neighbours.IsFailure)
                        return Result.Failure<string>(neighbours.Error);

                    if (RankService.TryGenerateBetween(neighbours.Value.Left, neighbours.Value.Right, out var between))
                    {
                        await tx.CommitAsync(cancellationToken);
                        return between;
                    }

                    if (attempt == normalizations.Length)
                        break;

                    Result normalized = await normalizations[attempt](neighbours.Value);
                    if (normalized.IsFailure)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        return Result.Failure<string>(normalized.Error);
                    }
                }

                await tx.RollbackAsync(cancellationToken);
                return Result.Failure<string>(RankingErrors.RankExhausted);
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

    private sealed record Neighbours(string? Left, string? Right);

    private async Task<Result<Neighbours>> FindNeighboursAsync(
        IQueryable<TEntity> siblings, int? beforeId, CancellationToken cancellationToken)
    {
        if (!beforeId.HasValue)
        {
            string? last = await siblings
                .OrderByDescending(s => s.Rank)
                .Select(s => s.Rank)
                .FirstOrDefaultAsync(cancellationToken);
            return new Neighbours(last, null);
        }

        string? right = await siblings
            .Where(s => s.Id == beforeId.Value)
            .Select(s => s.Rank)
            .FirstOrDefaultAsync(cancellationToken);
        if (right == null)
            return Result.Failure<Neighbours>(GetNotFoundError(beforeId.Value));

        string? left = await siblings
            .Where(s => s.Rank.CompareTo(right) < 0)
            .OrderByDescending(s => s.Rank)
            .Select(s => s.Rank)
            .FirstOrDefaultAsync(cancellationToken);
        return new Neighbours(left, right);
    }

    private async Task ApplyRanksAsync(IReadOnlyList<(int Id, string Rank)> ranks, CancellationToken cancellationToken)
    {
        foreach ((int id, _) in ranks)
        {
            string placeholder = TemporaryRank(id);
            await Entities
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Rank, placeholder), cancellationToken);
        }

        foreach ((int id, string rank) in ranks)
        {
            await Entities
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Rank, rank), cancellationToken);
        }
    }

    private static string TemporaryRank(int id) =>
        "~" + id.ToString(CultureInfo.InvariantCulture);

    public async Task<Result> NormalizeLocallyAsync(int groupId, string? leftRank, string? rightRank, CancellationToken cancellationToken = default)
    {
        if (leftRank == null && rightRank == null)
            return Result.Failure(RankingErrors.InvalidNormalizationRange);

        IExecutionStrategy strategy = DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var globalLockKey = GetGlobalLockKey();
                var groupLockKey = GetGroupLockKey(groupId);
                await DbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})",
                    [globalLockKey, groupLockKey], cancellationToken);

                Result result = await NormalizeLocallyInternalAsync(groupId, leftRank, rightRank, cancellationToken);

                if (result.IsSuccess)
                    await tx.CommitAsync(cancellationToken);
                else
                    await tx.RollbackAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to normalize locally {EntityName} ranks.", typeof(TEntity).Name);
                await tx.RollbackAsync(cancellationToken);
                return Result.Failure(RankingErrors.RankExhausted);
            }
        });
    }

    private async Task<Result> NormalizeLocallyInternalAsync(int groupId, string? leftRank, string? rightRank, CancellationToken cancellationToken)
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

        var middle = await Entities
            .AsNoTracking()
            .Where(GroupFilter(groupId))
            .Where(s => !s.IsDeleted)
            .Where(s => (leftRank == null || s.Rank.CompareTo(leftRank) >= 0) && (rightRank == null || s.Rank.CompareTo(rightRank) <= 0))
            .OrderBy(s => s.Rank)
            .Select(s => new EntityRankDto(s.Id, s.Rank))
            .ToListAsync(cancellationToken);

        var items = left.Concat(middle).Concat(right).OrderBy(s => s.Rank).ToList();
        if (items.Count < 2)
            return Result.Success();

        var newRanks = RankService.GenerateBalancedBetween(items.Count, items.First().Rank, items.Last().Rank);

        await ApplyRanksAsync(items.Select(i => i.Id).Zip(newRanks).ToList(), cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NormalizeGloballyAsync(int? groupId, CancellationToken cancellationToken = default)
    {
        IExecutionStrategy strategy = DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (groupId.HasValue)
                {
                    var globalLockKey = GetGlobalLockKey();
                    var groupLockKey = GetGroupLockKey(groupId.Value);
                    await DbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})",
                        [globalLockKey, groupLockKey], cancellationToken);

                    await NormalizeGroupInternalAsync(groupId.Value, cancellationToken);
                }
                else
                {
                    var globalLockKey = GetGlobalLockKey();
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

    private async Task<Result> NormalizeGroupInternalAsync(int groupId, CancellationToken cancellationToken)
    {
        var items = await Entities
            .AsNoTracking()
            .OrderBy(s => s.Rank)
            .Where(GroupFilter(groupId))
            .Where(s => !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            return Result.Success();

        var ranks = RankService.GenerateBalanced(items.Count);

        await ApplyRanksAsync(items.Zip(ranks).ToList(), cancellationToken);
        return Result.Success();
    }

    private async Task NormalizeAllGroupsAsync(CancellationToken cancellationToken)
    {
        List<int> groupIds = await Entities
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Select(GroupKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (int groupId in groupIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await NormalizeGroupInternalAsync(groupId, cancellationToken);
        }
    }

    private static long GetGlobalLockKey() =>
        (long)StableHash(typeof(TEntity).FullName!) << 32;

    private static long GetGroupLockKey(int groupId) =>
        ((long)StableHash(typeof(TEntity).FullName!) << 32) | (uint)groupId;

    private static uint StableHash(string s)
    {
        var hash = 2166136261u;
        foreach (var c in s)
        {
            hash ^= (byte)c;
            hash *= 16777619u;
        }
        return hash;
    }
}
