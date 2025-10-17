using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListCreatedDomainEvent : IDomainEvent<List>
{
    public int Id { get; private set; }

    public void SetEntity(List entity) => Id = entity.Id;
}
