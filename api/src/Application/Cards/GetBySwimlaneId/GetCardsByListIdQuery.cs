using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.GetBySwimlaneId;

public sealed record GetCardsBySwimlaneIdQuery(int Id) : IQuery<CardsResponse>;