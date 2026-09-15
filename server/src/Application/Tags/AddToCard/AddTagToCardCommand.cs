using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Tags.AddToCard;

public sealed record AddTagToCardCommand(int BoardId, int CardId, int TagId) : ICommand;
