using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record class SwimlaneCreatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Height,
    string Rank,
    string? ConnectionId) : IDomainEvent;
