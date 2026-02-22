using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Snapflow.Common;
using Snapflow.Infrastructure.Common;
using System.Data.Common;

namespace Snapflow.Infrastructure.Persistence.Interceptors;

internal sealed class DispatchDomainEventsInterceptor(
    DomainEventsBuffer buffer,
    IDomainEventsDispatcher dispatcher) 
    : SaveChangesInterceptor, IDbTransactionInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            CollectDomainEvents(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            await DispatchEventsAsync(cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public async Task TransactionCommittedAsync(
        DbTransaction transaction, 
        TransactionEndEventData eventData, 
        CancellationToken cancellationToken = default)
    {
        await DispatchEventsAsync(cancellationToken);
    }

    private void CollectDomainEvents(DbContext context)
    {
        var domainEvents = context.ChangeTracker
            .Entries<IEntity>()
            .Select(entry => entry.Entity)
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
