using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.Delete;

public sealed record DeleteCardCommand(int Id) : ICommand;