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
    string? ConnectionId) : IDomainEvent;

public sealed record CardUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    string Description,
    string? ConnectionId) : IDomainEvent;

public sealed record CardMovedDomainEvent(
    int Id,
    int BoardId,
    int ListId,
    string Rank,
    string? ConnectionId) : IDomainEvent;

public sealed record CardDeletedDomainEvent(
    int Id,
    int BoardId,
    string? ConnectionId) : IDomainEvent;
