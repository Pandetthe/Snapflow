using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Cards.GetBySwimlaneId.GetCardsBySwimlaneIdResponse;

namespace Snapflow.Application.Cards.GetBySwimlaneId;

public sealed record GetCardsBySwimlaneIdQuery(int BoardId, int Id) : IQuery<IReadOnlyList<CardDto>>;