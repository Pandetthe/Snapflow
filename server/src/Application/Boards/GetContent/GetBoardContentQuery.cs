using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Boards.GetContent;

public sealed record GetBoardContentQuery(int Id) : IQuery<GetBoardContentResponse>;
