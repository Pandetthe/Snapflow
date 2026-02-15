using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.Update;

public sealed record UpdateBoardCommand(int Id, string Title, string Description) : ICommand;