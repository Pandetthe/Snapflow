using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListCreatedDomainEvent(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Title,
    int? Width,
    string Rank,
    string? ConnectionId) : IDomainEvent;

public sealed record ListUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Width,
    string? ConnectionId) : IDomainEvent;

public sealed record ListMovedDomainEvent(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Rank,
    string? ConnectionId) : IDomainEvent;

public sealed record ListDeletedDomainEvent(
    int Id,
    int BoardId,
    string? ConnectionId) : IDomainEvent;
