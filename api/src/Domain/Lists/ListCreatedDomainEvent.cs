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
