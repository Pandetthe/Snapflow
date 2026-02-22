using Snapflow.Common;

namespace Snapflow.Infrastructure.Persistence;

internal sealed class DomainEventsBuffer
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddRange(IEnumerable<IDomainEvent> events)
    {
        _domainEvents.AddRange(events);
    }

    public void Clear()
    {
        _domainEvents.Clear();
    }
}
