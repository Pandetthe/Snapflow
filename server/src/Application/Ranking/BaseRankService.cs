using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Ranking;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Ranking;
using System.Globalization;
using System.Linq.Expressions;

namespace Snapflow.Application.Ranking;

internal abstract class BaseRankService<TEntity>(
    IAppDbContext dbContext,
    IRankService rankService) : IEntityRankService<TEntity>
    where TEntity : class, IEntity, IRankable
{
    protected IAppDbContext DbContext => dbContext;
    protected IRankService RankService => rankService;

    protected abstract DbSet<TEntity> Entities { get; }
    protected abstract Expression<Func<TEntity, int>> GroupKey { get; }
    protected abstract Expression<Func<TEntity, bool>> GroupFilter(int groupId);
    protected abstract Error GetNotFoundError(int id);

    public async Task<Result<string>> GenerateRankAsync(int groupId, int? movingId, int? beforeId,
        CancellationToken cancellationToken = default)
    {
        if (DbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("A rank must be generated inside the transaction that saves it.");

        await DbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})",
            [GetGlobalLockKey(), GetGroupLockKey(groupId)], cancellationToken);

        var baseQuery = Entities
            .AsNoTracking()
            .Where(GroupFilter(groupId))
            .Where(s => movingId == null || s.Id != movingId);

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
                return neighbours.Error;

            if (RankService.TryGenerateBetween(neighbours.Value.Left, neighbours.Value.Right, out var between))
                return between;

            if (attempt == normalizations.Length)
                return RankingErrors.RankExhausted;

            await normalizations[attempt](neighbours.Value);
        }
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
            return GetNotFoundError(beforeId.Value);

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

    private async Task<Result> NormalizeLocallyInternalAsync(int groupId, string? leftRank, string? rightRank, CancellationToken cancellationToken)
    {
        List<EntityRankDto> left = [];
        List<EntityRankDto> right = [];

        if (leftRank != null)
        {
            left = await Entities
                .AsNoTracking()
                .Where(GroupFilter(groupId))
                .Where(s => s.Rank.CompareTo(leftRank) < 0)
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
                .Where(s => s.Rank.CompareTo(rightRank) > 0)
                .OrderBy(s => s.Rank)
                .Select(s => new EntityRankDto(s.Id, s.Rank))
                .Take(20)
                .ToListAsync(cancellationToken);
        }

        var middle = await Entities
            .AsNoTracking()
            .Where(GroupFilter(groupId))
            .Where(s => (leftRank == null || s.Rank.CompareTo(leftRank) >= 0) && (rightRank == null || s.Rank.CompareTo(rightRank) <= 0))
            .OrderBy(s => s.Rank)
            .Select(s => new EntityRankDto(s.Id, s.Rank))
            .ToListAsync(cancellationToken);

        var items = left.Concat(middle).Concat(right).OrderBy(s => s.Rank).ToList();
        if (items.Count < 2)
            return Result.Success();

        if (!RankService.TryGenerateBalancedBetween(items.Count, items[0].Rank, items[^1].Rank, out var newRanks))
            return RankingErrors.RankExhausted;

        await ApplyRanksAsync(items.Select(i => i.Id).Zip(newRanks).ToList(), cancellationToken);
        return Result.Success();
    }

    public Task<Result> NormalizeGloballyAsync(int? groupId, CancellationToken cancellationToken = default) =>
        DbContext.InTransactionAsync(async () =>
        {
            if (groupId.HasValue)
            {
                await DbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock_shared({0}), pg_advisory_xact_lock({1})",
                    [GetGlobalLockKey(), GetGroupLockKey(groupId.Value)], cancellationToken);

                return await NormalizeGroupInternalAsync(groupId.Value, cancellationToken);
            }

            await DbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0})", [GetGlobalLockKey()], cancellationToken);

            return await NormalizeAllGroupsAsync(cancellationToken);
        }, cancellationToken);

    private async Task<Result> NormalizeGroupInternalAsync(int groupId, CancellationToken cancellationToken)
    {
        var items = await Entities
            .AsNoTracking()
            .OrderBy(s => s.Rank)
            .Where(GroupFilter(groupId))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            return Result.Success();

        if (!RankService.TryGenerateBalanced(items.Count, out var ranks))
            return RankingErrors.RankExhausted;

        await ApplyRanksAsync(items.Zip(ranks).ToList(), cancellationToken);
        return Result.Success();
    }

    private async Task<Result> NormalizeAllGroupsAsync(CancellationToken cancellationToken)
    {
        List<int> groupIds = await Entities
            .AsNoTracking()
            .Select(GroupKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (int groupId in groupIds)
        {
            Result normalized = await NormalizeGroupInternalAsync(groupId, cancellationToken);
            if (normalized.IsFailure)
                return normalized;
        }

        return Result.Success();
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
