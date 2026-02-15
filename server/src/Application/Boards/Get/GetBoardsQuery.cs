using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Boards.Get.GetBoardsResponse;

namespace Snapflow.Application.Boards.Get;

public sealed record GetBoardsQuery(string? Title) : IQuery<IReadOnlyList<BoardDto>>;