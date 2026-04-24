using Microsoft.AspNetCore.OutputCaching;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Presentation.Caching;

namespace Snapflow.Presentation.Hubs.Board.CacheInvalidationHandlers;

internal sealed class ContentCacheInvalidator(IOutputCacheStore store) :
    IDomainEventHandler<CardCreatedDomainEvent>,
    IDomainEventHandler<CardUpdatedDomainEvent>,
    IDomainEventHandler<CardMovedDomainEvent>,
    IDomainEventHandler<CardDeletedDomainEvent>,
    IDomainEventHandler<ListCreatedDomainEvent>,
    IDomainEventHandler<ListUpdatedDomainEvent>,
    IDomainEventHandler<ListMovedDomainEvent>,
    IDomainEventHandler<ListDeletedDomainEvent>,
    IDomainEventHandler<SwimlaneCreatedDomainEvent>,
    IDomainEventHandler<SwimlaneUpdatedDomainEvent>,
    IDomainEventHandler<SwimlaneMovedDomainEvent>,
    IDomainEventHandler<SwimlaneDeletedDomainEvent>
{
    public Task Handle(CardCreatedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(CardUpdatedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(CardMovedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(CardDeletedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(ListCreatedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(ListUpdatedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(ListMovedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(ListDeletedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(SwimlaneCreatedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(SwimlaneUpdatedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(SwimlaneMovedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);
    public Task Handle(SwimlaneDeletedDomainEvent e, CancellationToken ct) => Evict(e.BoardId, ct);

    private Task Evict(int boardId, CancellationToken ct) =>
        store.EvictByTagAsync(CacheTags.Board(boardId), ct).AsTask();
}
