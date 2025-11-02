using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.GetByBoardId;

public sealed record GetCardsByBoardIdQuery(int Id) : IQuery<CardsResponse>;