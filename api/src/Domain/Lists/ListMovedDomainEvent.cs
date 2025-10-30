using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListMovedDomainEvent(int Id, int BoardId, int SwimlaneId, string Rank) : IDomainEvent;
