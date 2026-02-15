using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public sealed record SwimlaneUpdatedDomainEvent(
    int Id,
    int BoardId,
    string Title,
    int? Height,
    string? ConnectionId) : IDomainEvent;
