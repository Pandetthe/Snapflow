using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record SwimlaneCreatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Height,
    string Rank,
    string? ConnectionId) : IDomainEvent;

public sealed record SwimlaneUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Height,
    string? ConnectionId) : IDomainEvent;

public sealed record SwimlaneMovedDomainEvent(
    int Id,
    int BoardId,
    string Rank,
    string? ConnectionId) : IDomainEvent;

public sealed record SwimlaneDeletedDomainEvent(
    int Id,
    int BoardId,
    string? ConnectionId) : IDomainEvent;
