using Microsoft.AspNetCore.OutputCaching;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Caching;

namespace Snapflow.Presentation.Hubs.Board.CacheInvalidationHandlers;

internal sealed class BoardCacheInvalidator(IOutputCacheStore store) :
    IDomainEventHandler<BoardCreatedDomainEvent>,
    IDomainEventHandler<BoardUpdatedDomainEvent>,
    IDomainEventHandler<BoardDeletedDomainEvent>
{
    public Task Handle(BoardCreatedDomainEvent e, CancellationToken ct) =>
        store.EvictByTagAsync(CacheTags.User(e.CreatedById), ct).AsTask();

    public Task Handle(BoardUpdatedDomainEvent e, CancellationToken ct) =>
        store.EvictByTagAsync(CacheTags.Board(e.Id), ct).AsTask();

    public Task Handle(BoardDeletedDomainEvent e, CancellationToken ct) =>
        store.EvictByTagAsync(CacheTags.Board(e.Id), ct).AsTask();
}
