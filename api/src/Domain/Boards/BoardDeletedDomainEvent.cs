using Snapflow.Common;

namespace Snapflow.Domain.Boards;

public sealed class BoardDeletedDomainEvent : IDomainEvent
{
    public int Id { get; private set; }
}
