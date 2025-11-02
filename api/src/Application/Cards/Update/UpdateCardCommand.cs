using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.Update;

public sealed record UpdateCardCommand(int Id, string Title, string Description) : ICommand;