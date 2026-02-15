namespace Snapflow.Common;

public interface IEntity
{
    IReadOnlyCollection<Func<object, IDomainEvent>> DomainEvents { get; }
    void ClearDomainEvents();
}

public interface IEntity<out TKey> : IEntity where TKey : IEquatable<TKey>
{
    TKey Id { get; }
}