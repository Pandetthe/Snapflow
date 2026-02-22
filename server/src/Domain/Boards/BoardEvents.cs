using Snapflow.Common;

namespace Snapflow.Domain.Boards;

public sealed record BoardCreatedDomainEvent(
    int Id,
    string Title,
    int CreatedById,
    string? ConnectionId) : IDomainEvent;

public sealed record BoardUpdatedDomainEvent(
    int Id,
    string Title,
    string Description,
    string? ConnectionId) : IDomainEvent;

public sealed record BoardDeletedDomainEvent(
    int Id,
    string? ConnectionId) : IDomainEvent;
