using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListUpdatedDomainEvent(int Id, int BoardId, int SwimlaneId, string Title) : IDomainEvent;
