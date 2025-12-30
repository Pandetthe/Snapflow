using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardMovedDomainEvent(int Id, int BoardId, int ListId, string Rank) : IDomainEvent;
