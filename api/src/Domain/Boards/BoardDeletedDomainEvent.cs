using Snapflow.Common;

namespace Snapflow.Domain.Boards;

public sealed record BoardDeletedDomainEvent(
    int Id,
    string? ConnectionId) : IDomainEvent;