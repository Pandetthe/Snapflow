using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardCreatedDomainEvent(
    int Id,
    int BoardId,
    int ListId,
    string Title,
    string Description,
    string Rank,
    string? ConnectionId) : IDomainEvent;
