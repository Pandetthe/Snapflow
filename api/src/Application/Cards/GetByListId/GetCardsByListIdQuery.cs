using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.GetByListId;

public sealed record GetCardsByListIdQuery(int Id) : IQuery<CardsResponse>;