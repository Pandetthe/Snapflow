using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.GetById;

public sealed record GetBoardByIdQuery(int BoardId) : IQuery<BoardResponse>;
