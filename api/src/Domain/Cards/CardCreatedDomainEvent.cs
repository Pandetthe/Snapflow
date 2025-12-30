using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardCreatedDomainEvent(int BoardId, int ListId, 
    string Title, string Description, string Rank) : IDomainEvent<Card>
{
    public int Id { get; private set; }

    public void SetEntity(Card entity) => Id = entity.Id;
}
