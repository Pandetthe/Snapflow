using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Cards.Create;

public sealed record CreateCardCommand(int BoardId, int SwimlaneId, int ListId, string Title, string Description) : ICommand<int>;