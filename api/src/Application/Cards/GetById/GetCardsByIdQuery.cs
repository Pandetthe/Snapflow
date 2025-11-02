using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.GetById;

public sealed record GetCardsByIdQuery(int Id) : IQuery<CardResponse>;