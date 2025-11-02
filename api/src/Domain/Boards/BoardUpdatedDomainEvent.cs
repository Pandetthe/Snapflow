using Snapflow.Common;

namespace Snapflow.Domain.Boards;

public sealed record BoardUpdatedDomainEvent(int Id, string Title, string Description) : IDomainEvent;