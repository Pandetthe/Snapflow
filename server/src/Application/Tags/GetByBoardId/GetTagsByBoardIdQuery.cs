using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Tags.GetByBoardId.GetTagsByBoardIdResponse;

namespace Snapflow.Application.Tags.GetByBoardId;

public sealed record GetTagsByBoardIdQuery(int Id) : IQuery<IReadOnlyList<TagDto>>;
