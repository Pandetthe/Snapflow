using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.Move;

public sealed record MoveCardCommand(int Id, int? BeforeId) : ICommand;