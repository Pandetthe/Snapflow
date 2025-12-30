using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListUpdatedDomainEvent(int Id, int BoardId, string Title, int? Width) : IDomainEvent;
