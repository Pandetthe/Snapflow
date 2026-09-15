using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Tags.RemoveFromCard;

public sealed record RemoveTagFromCardCommand(int BoardId, int CardId, int TagId) : ICommand;
