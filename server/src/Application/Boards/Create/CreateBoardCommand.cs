using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.Create;

public sealed record CreateBoardCommand(string Title, string Description) : ICommand<int>;