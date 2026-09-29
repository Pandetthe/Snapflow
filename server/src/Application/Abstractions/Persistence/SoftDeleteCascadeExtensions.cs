using Microsoft.EntityFrameworkCore;
using Snapflow.Common;

namespace Snapflow.Application.Abstractions.Persistence;

public static class SoftDeleteCascadeExtensions
{
    extension(IAppDbContext dbContext)
    {
        public async Task CascadeBoardDeletionAsync(
            int boardId, int deletedById, DateTimeOffset deletedAt, CancellationToken cancellationToken)
        {
            await MarkDeletedAsync(dbContext.Swimlanes.Where(s => s.BoardId == boardId), deletedById, deletedAt, cancellationToken);
            await MarkDeletedAsync(dbContext.Lists.Where(l => l.BoardId == boardId), deletedById, deletedAt, cancellationToken);
            await MarkDeletedAsync(dbContext.Cards.Where(c => c.BoardId == boardId), deletedById, deletedAt, cancellationToken);
            await dbContext.Tags
                .Where(t => t.BoardId == boardId)
                .ExecuteUpdateAsync(t => t
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, deletedAt)
                    .SetProperty(x => x.DeletedById, deletedById),
                    cancellationToken);
        }

        public async Task CascadeSwimlaneDeletionAsync(
            int swimlaneId, int deletedById, DateTimeOffset deletedAt, CancellationToken cancellationToken)
        {
            await MarkDeletedAsync(dbContext.Lists.Where(l => l.SwimlaneId == swimlaneId), deletedById, deletedAt, cancellationToken);
            await MarkDeletedAsync(dbContext.Cards.Where(c => c.SwimlaneId == swimlaneId), deletedById, deletedAt, cancellationToken);
        }

        public Task CascadeListDeletionAsync(
            int listId, int deletedById, DateTimeOffset deletedAt, CancellationToken cancellationToken) =>
            MarkDeletedAsync(dbContext.Cards.Where(c => c.ListId == listId), deletedById, deletedAt, cancellationToken);
    }

    private static Task<int> MarkDeletedAsync<TEntity>(
        IQueryable<TEntity> children, int deletedById, DateTimeOffset deletedAt, CancellationToken cancellationToken)
        where TEntity : class, ICascadeSoftDeletable =>
        children.ExecuteUpdateAsync(e => e
            .SetProperty(x => x.IsDeleted, true)
            .SetProperty(x => x.DeletedAt, deletedAt)
            .SetProperty(x => x.DeletedById, deletedById)
            .SetProperty(x => x.DeletedByCascade, true),
            cancellationToken);
}
