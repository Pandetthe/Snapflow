using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.Update;

public sealed record UpdateCardCommand(int BoardId, int Id, string Title, string Description) : ICommand<UpdateCardResponse>;