using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.GetById;

public sealed record GetCardByIdQuery(int BoardId, int Id) : IQuery<GetCardByIdResponse>;