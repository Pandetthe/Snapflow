using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListDeletedDomainEvent(int Id, int BoardId, int SwimlaneId) : IDomainEvent;
