using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.GetPublic;

public sealed record GetPublicBoardsQuery : IQuery<IReadOnlyList<GetPublicBoardsResponse>>;
