using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardCreatedDomainEvent : IDomainEvent<Card>
{
    public int Id { get; private set; }

    public void SetEntity(Card entity) => Id = entity.Id;
}
