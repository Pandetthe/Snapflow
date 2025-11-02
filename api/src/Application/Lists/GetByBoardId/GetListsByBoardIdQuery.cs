using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.GetByBoardId;

public sealed record GetListsByBoardIdQuery(int Id) : IQuery<ListsResponse>;