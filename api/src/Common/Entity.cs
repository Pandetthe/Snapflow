using System.ComponentModel.DataAnnotations.Schema;

namespace Snapflow.Common;

public abstract class Entity<TEntity> : IEntity
    where TEntity : Entity<TEntity>
{
    private readonly List<Func<object, IDomainEvent>> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<Func<object, IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();

    public void Raise(Func<TEntity, IDomainEvent> blueprint) =>
        _domainEvents.Add(obj => blueprint((TEntity)obj));

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class Entity<TKey, TEntity> : IEntity<TKey>
    where TKey : IEquatable<TKey>
    where TEntity : Entity<TKey, TEntity>
{
    private readonly List<Func<object, IDomainEvent>> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<Func<object, IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();

    public TKey Id { get; protected set; } = default!;

    public void Raise(Func<TEntity, IDomainEvent> blueprint) =>
        _domainEvents.Add(obj => blueprint((TEntity)obj));

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj) =>
        obj is Entity<TKey, TEntity> other && Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}