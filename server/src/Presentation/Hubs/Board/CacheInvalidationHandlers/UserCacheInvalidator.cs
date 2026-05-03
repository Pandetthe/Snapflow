using Microsoft.AspNetCore.OutputCaching;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Presentation.Caching;

namespace Snapflow.Presentation.Hubs.Board.CacheInvalidationHandlers;

internal sealed class UserCacheInvalidator(IOutputCacheStore store) :
    IDomainEventHandler<UserProfileUpdatedDomainEvent>,
    IDomainEventHandler<UserDeletedDomainEvent>
{
    public Task Handle(UserProfileUpdatedDomainEvent e, CancellationToken ct) =>
        EvictUserCacheAsync(e.UserId, ct);

    public Task Handle(UserDeletedDomainEvent e, CancellationToken ct) =>
        EvictUserCacheAsync(e.UserId, ct);

    private Task EvictUserCacheAsync(int userId, CancellationToken ct) =>
        Task.WhenAll(
            store.EvictByTagAsync(CacheTags.User(userId), ct).AsTask(),
            store.EvictByTagAsync(CacheTags.Avatar(userId), ct).AsTask());
}
