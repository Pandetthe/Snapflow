using Snapflow.Common;

namespace Snapflow.Domain.Cards;

public sealed record CardUpdatedDomainEvent(int Id, int BoardId,
    string Title, string Description) : IDomainEvent;

