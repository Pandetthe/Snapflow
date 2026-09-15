using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Snapflow.Common;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Infrastructure.Persistence.Interceptors;

internal sealed class DispatchDomainEventsInterceptor(
    DomainEventsBuffer buffer,
    IDomainEventsDispatcher dispatcher)
    : SaveChangesInterceptor, IDbTransactionInterceptor
{
    // Entities the save is about to delete. EF detaches them once it accepts the changes, so by the
    // time the events are collected they are no longer in the change tracker and their events would
    // be lost. They are collected here rather than dispatched, so a failed save still raises nothing.
    private readonly List<IEntity> _deletedEntities = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ChangeTracker changeTracker = eventData.Context.ChangeTracker;

            // An entity taken out of a parent's collection is only marked deleted once the changes
            // are detected, and EF does not promise to have done that before this runs. Detecting is
            // idempotent, and the guard keeps a context that opted out of it behaving as it asked.
            if (changeTracker.AutoDetectChangesEnabled)
                changeTracker.DetectChanges();

            _deletedEntities.AddRange(changeTracker
                .Entries<IEntity>()
                .Where(entry => entry.State == EntityState.Deleted)
                .Select(entry => entry.Entity));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            CollectDomainEvents(eventData.Context);
        }

        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            await DispatchEventsAsync(cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        // Nothing was deleted after all, so the held entities must not reach the next save.
        _deletedEntities.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    public async Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            CollectDomainEvents(eventData.Context);
        }

        await DispatchEventsAsync(cancellationToken);
    }

    private void CollectDomainEvents(DbContext context)
    {
        // Inserted entities only get their key during the save, and their event blueprints read it,
        // so the blueprints are invoked here rather than before the save.
        var entities = context.ChangeTracker
            .Entries<IEntity>()
            .Select(entry => entry.Entity)
            .Concat(_deletedEntities)
            .ToList();

        _deletedEntities.Clear();

        // An entity reached twice contributes nothing the second time, its events having been cleared.
        var domainEvents = entities
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.Select(de => de.Invoke(entity)).ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();

        buffer.AddRange(domainEvents);
    }

    private async Task DispatchEventsAsync(CancellationToken cancellationToken)
    {
        if (buffer.DomainEvents.Count == 0)
        {
            return;
        }

        var events = buffer.DomainEvents.ToList();
        buffer.Clear();

        await dispatcher.DispatchAsync(events, cancellationToken);
    }
}
