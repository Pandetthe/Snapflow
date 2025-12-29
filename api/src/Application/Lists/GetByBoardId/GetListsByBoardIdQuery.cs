using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Lists.GetByBoardId.GetListsByBoardId;

namespace Snapflow.Application.Lists.GetByBoardId;

public sealed record GetListsByBoardIdQuery(int Id) : IQuery<IReadOnlyList<ListDto>>;