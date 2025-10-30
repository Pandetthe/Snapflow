using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record SwimlaneMovedDomainEvent(int Id, int BoardId, string Rank) : IDomainEvent;
