using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListCreatedDomainEvent(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Title,
    int? Width,
    string Rank,
    int CreatedById,
    string CreatedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record ListUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Width,
    int UpdatedById,
    string UpdatedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record ListMovedDomainEvent(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Rank,
    int MovedById,
    string MovedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record ListDeletedDomainEvent(
    int Id,
    int BoardId,
    int DeletedById,
    string DeletedByUserName,
    string? ConnectionId) : IDomainEvent;
