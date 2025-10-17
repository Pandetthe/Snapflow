using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.Delete;

public sealed record DeleteBoardCommand(int BoardId) : ICommand;