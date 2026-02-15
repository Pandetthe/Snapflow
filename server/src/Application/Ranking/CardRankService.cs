using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Ranking;

namespace Snapflow.Application.Ranking;

internal sealed class CardRankService(
    ILogger<CardRankService> logger,
    IAppDbContext dbContext,
    IRankService rankService) : IEntityRankService<Card>
{
    public async Task<Result<string>> GenerateRankAsync(int groupId, int? movingId, int? beforeId,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.Cards
            .AsNoTracking()
            .Where(s => s.ListId == groupId && !s.IsDeleted && (movingId == null || s.Id != movingId));

        if (!await baseQuery.AnyAsync(cancellationToken))
            return rankService.GenerateInitial();

        string? leftRank = null;
        string? rightRank = null;

        if (beforeId.HasValue)
        {
            rightRank = await baseQuery
                .Where(s => s.Id == beforeId.Value)
                .Select(s => s.Rank)
                .FirstOrDefaultAsync(cancellationToken);
            if (rightRank == null)
                return Result.Failure<string>(CardErrors.NotFound(beforeId.Value));

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
                return rankService.GenerateInitial();
        }

        if (rankService.TryGenerateBetween(leftRank, rightRank, out var between))
            return between;

        Result result = await NormalizeLocallyAsync(groupId, leftRank, rightRank, cancellationToken);

        if (result.IsSuccess && rankService.TryGenerateBetween(leftRank, rightRank, out between))
            return between;

        result = await NormalizeGloballyAsync(groupId, cancellationToken);

        if (result.IsSuccess && rankService.TryGenerateBetween(leftRank, rightRank, out between))
            return between;

        return Result.Failure<string>(result.Error);
    }

    private sealed record CardRankDto(int Id, string Rank);

    public async Task<Result> NormalizeLocallyAsync(int groupId, string? leftRank, string? rightRank, CancellationToken cancellationToken = default)
    {
        if (leftRank == null && rightRank == null)
            return Result.Failure(RankingErrors.InvalidNormalizationRange);
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<CardRankDto> left = [];
            List<CardRankDto> right = [];
            if (leftRank != null)
            {
                left = await dbContext.Cards
                    .AsNoTracking()
                    .Where(s => s.ListId == groupId && s.Rank.CompareTo(leftRank) < 0 && !s.IsDeleted)
                    .OrderByDescending(s => s.Rank)
                    .Select(s => new CardRankDto(s.Id, s.Rank))
                    .Take(20)
                    .ToListAsync(cancellationToken);
            }
            if (rightRank != null)
            {
                right = await dbContext.Cards
                    .AsNoTracking()
                    .Where(s => s.ListId == groupId && s.Rank.CompareTo(rightRank) > 0 && !s.IsDeleted)
                    .OrderBy(s => s.Rank)
                    .Select(s => new CardRankDto(s.Id, s.Rank))
                    .Take(20)
                    .ToListAsync(cancellationToken);
            }
            List<CardRankDto> middle = await dbContext.Cards
                .AsNoTracking()
                .Where(s => s.ListId == groupId && !s.IsDeleted)
                .Where(s => (leftRank == null || s.Rank.CompareTo(leftRank) >= 0) && (rightRank == null || s.Rank.CompareTo(rightRank) <= 0))
                .OrderBy(s => s.Rank)
                .Select(s => new CardRankDto(s.Id, s.Rank))
                .ToListAsync(cancellationToken);
            var cards = left.Concat(middle).Concat(right).OrderBy(s => s.Rank).ToList();
            if (cards.Count < 2)
            {
                await tx.CommitAsync(cancellationToken);
                return Result.Success();
            }
            var newRanks = rankService.GenerateBalancedBetween(cards.Count, cards.First().Rank, cards.Last().Rank);
            foreach (var (card, newRank) in cards.Zip(newRanks))
            {
                await dbContext.Cards
                    .Where(s => s.Id == card.Id && s.Rank == card.Rank)
                    .ExecuteUpdateAsync(s => s.SetProperty(sl => sl.Rank, newRank), cancellationToken);
            }
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to normalize locally card ranks.");
            await tx.RollbackAsync(cancellationToken);
            return Result.Failure(RankingErrors.RankExhausted);
        }
    }

    public async Task<Result> NormalizeGloballyAsync(int? groupId, CancellationToken cancellationToken = default)
    {
        var tableName = dbContext.Cards.EntityType.GetTableName();
        var schema = dbContext.Cards.EntityType.GetSchema();
        var fullTableName = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";

        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
            await dbContext.Database.ExecuteSqlRawAsync(
                $"LOCK TABLE {fullTableName} IN ACCESS EXCLUSIVE MODE;", cancellationToken);
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.

            if (groupId.HasValue)
            {
                var cards = await dbContext.Cards
                    .AsNoTracking()
                    .OrderBy(s => s.Rank)
                    .Where(s => s.ListId == groupId && !s.IsDeleted)
                    .Select(s => s.Id)
                    .ToListAsync(cancellationToken);
                if (cards.Count == 0)
                    return Result.Success();
                List<string> ranks = rankService.GenerateBalanced(cards.Count);

                foreach (var (cardId, rank) in cards.Zip(ranks))
                    await dbContext.Cards
                        .Where(s => s.Id == cardId)
                        .ExecuteUpdateAsync(s => s.SetProperty(sl => sl.Rank, rank), cancellationToken);
            }
            else
            {
                var listIds = await dbContext.Cards
                    .Where(s => !s.IsDeleted)
                    .Select(s => s.ListId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var bId in listIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var cards = await dbContext.Cards
                        .OrderBy(s => s.Rank)
                        .Where(s => s.ListId == bId && !s.IsDeleted)
                        .Select(s => s.Id)
                        .ToListAsync(cancellationToken);
                    if (cards.Count == 0)
                        continue;
                    List<string> ranks = rankService.GenerateBalanced(cards.Count);

                    foreach (var (cardId, rank) in cards.Zip(ranks))
                        await dbContext.Cards
                            .Where(s => s.Id == cardId)
                            .ExecuteUpdateAsync(s => s.SetProperty(sl => sl.Rank, rank), cancellationToken);
                }
            }

            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to normalize card ranks.");
            await tx.RollbackAsync(cancellationToken);
            return Result.Failure(RankingErrors.RankExhausted);
        }
    }
}
