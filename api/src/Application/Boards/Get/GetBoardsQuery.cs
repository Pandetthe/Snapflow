using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.Get;

public sealed record GetBoardsQuery(string? Title) : IQuery<List<BoardResponse>>;
