using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record SwimlaneCreatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Height,
    string Rank,
    int CreatedById,
    string CreatedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record SwimlaneUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Height,
    int UpdatedById,
    string UpdatedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record SwimlaneMovedDomainEvent(
    int Id,
    int BoardId,
    string Rank,
    int MovedById,
    string MovedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record SwimlaneDeletedDomainEvent(
    int Id,
    int BoardId,
    int DeletedById,
    string DeletedByUserName,
    string? ConnectionId) : IDomainEvent;
