using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.Create;

public sealed record CreateBoardCommand() : ICommand<int>
{
    public required string Title { get; init; }

    public string Description { get; init; } = "";
}