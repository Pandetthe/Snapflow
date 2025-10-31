using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardDeletedDomainEvent(int Id, int BoardId, int SwimlaneId, int ListId) : IDomainEvent;
