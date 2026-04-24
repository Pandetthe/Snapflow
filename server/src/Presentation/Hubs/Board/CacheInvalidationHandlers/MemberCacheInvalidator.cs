using Microsoft.AspNetCore.OutputCaching;
using Snapflow.Common;
using Snapflow.Domain.Members;
using Snapflow.Presentation.Caching;

namespace Snapflow.Presentation.Hubs.Board.CacheInvalidationHandlers;

internal sealed class MemberCacheInvalidator(IOutputCacheStore store) :
    IDomainEventHandler<MemberCreatedDomainEvent>,
    IDomainEventHandler<MemberRemovedDomainEvent>,
    IDomainEventHandler<MemberRoleChangedDomainEvent>
{
    public async Task Handle(MemberCreatedDomainEvent e, CancellationToken ct)
    {
        await store.EvictByTagAsync(CacheTags.Board(e.BoardId), ct);
        await store.EvictByTagAsync(CacheTags.User(e.UserId), ct);
    }

    public async Task Handle(MemberRemovedDomainEvent e, CancellationToken ct)
    {
        await store.EvictByTagAsync(CacheTags.Board(e.BoardId), ct);
        await store.EvictByTagAsync(CacheTags.User(e.UserId), ct);
    }

    public Task Handle(MemberRoleChangedDomainEvent e, CancellationToken ct) =>
        store.EvictByTagAsync(CacheTags.Board(e.BoardId), ct).AsTask();
}
