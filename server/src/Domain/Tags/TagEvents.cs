using Snapflow.Common;

namespace Snapflow.Domain.Tags;

public sealed record TagCreatedDomainEvent(int Id, int BoardId, string Title, string? ConnectionId) : IDomainEvent;
public sealed record TagUpdatedDomainEvent(int Id, int BoardId, string Title, string? ConnectionId) : IDomainEvent;
public sealed record TagDeletedDomainEvent(int Id, int BoardId, string? ConnectionId) : IDomainEvent;
