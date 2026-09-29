using Snapflow.Common;

namespace Snapflow.Domain.Tags;

public sealed record TagCreatedDomainEvent(int Id, int BoardId, string Title, TagColors Color,
    int CreatedById, string? ConnectionId) : IDomainEvent;

public sealed record TagUpdatedDomainEvent(int Id, int BoardId, string Title, TagColors Color,
    int UpdatedById, string? ConnectionId) : IDomainEvent;

public sealed record TagDeletedDomainEvent(int Id, int BoardId,
    int DeletedById, string? ConnectionId) : IDomainEvent;

public sealed record CardTagAddedDomainEvent(int CardId, int TagId, int BoardId,
    int AddedById, string? ConnectionId) : IDomainEvent;

public sealed record CardTagRemovedDomainEvent(int CardId, int TagId, int BoardId,
    int RemovedById, string? ConnectionId) : IDomainEvent;
