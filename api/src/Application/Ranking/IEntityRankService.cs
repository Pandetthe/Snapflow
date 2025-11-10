using Snapflow.Common;

namespace Snapflow.Application.Ranking;

public interface IEntityRankService<TEntity> where TEntity : IEntity
{
    Task<Result<string>> GenerateRankAsync(int groupId, int? movingId, int? beforeId,
        CancellationToken cancellationToken = default);

    Task<Result> NormalizeGloballyAsync(int? groupId, CancellationToken cancellationToken = default);
}
