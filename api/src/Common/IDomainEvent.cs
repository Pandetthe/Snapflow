namespace Snapflow.Common;

public interface IDomainEvent;

public interface IDomainEvent<T> : IDomainEvent where T : IEntity
{
    public void SetEntity(T entity);
}
