using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Cards.GetByListId.GetCardsByListIdResponse;

namespace Snapflow.Application.Cards.GetByListId;

public sealed record GetCardsByListIdQuery(int Id) : IQuery<IReadOnlyList<CardDto>>;