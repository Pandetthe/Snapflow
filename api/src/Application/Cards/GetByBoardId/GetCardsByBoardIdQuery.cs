using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Cards.GetByBoardId.GetCardsByBoardIdResponse;

namespace Snapflow.Application.Cards.GetByBoardId;

public sealed record GetCardsByBoardIdQuery(int Id) : IQuery<IReadOnlyList<CardDto>>;