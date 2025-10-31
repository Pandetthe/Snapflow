using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardMovedDomainEvent(int CardId, int BoardId, int SwimlaneId, int ListId, string Rank) : IDomainEvent;
