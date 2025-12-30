using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record class SwimlaneCreatedDomainEvent(int BoardId, string Title, int? Height, string Rank) : IDomainEvent<Swimlane>
{
    public int Id { get; private set; }

    public void SetEntity(Swimlane entity) => Id = entity.Id;
}
