using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record SwimlaneDeletedDomainEvent(int Id, int BoardId) : IDomainEvent;
