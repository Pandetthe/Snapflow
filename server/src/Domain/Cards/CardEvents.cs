using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardCreatedDomainEvent(
    int Id,
    int BoardId,
    int SwimlaneId,
    int ListId,
    string Title,
    string Description,
    string Rank,
    DateTimeOffset CreatedAt,
    int CreatedById,
    string CreatedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record CardUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    string Description,
    int UpdatedById,
    string UpdatedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record CardMovedDomainEvent(
    int Id,
    int BoardId,
    int ListId,
    string Rank,
    int MovedById,
    string MovedByUserName,
    string? ConnectionId) : IDomainEvent;

public sealed record CardDeletedDomainEvent(
    int Id,
    int BoardId,
    int DeletedById,
    string DeletedByUserName,
    string? ConnectionId) : IDomainEvent;
