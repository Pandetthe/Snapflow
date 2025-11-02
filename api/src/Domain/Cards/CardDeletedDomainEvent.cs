using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardDeletedDomainEvent(int Id, int BoardId) : IDomainEvent;
