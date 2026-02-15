using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Swimlanes.GetByBoardId.GetSwimlanesByBoardIdResponse;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

public sealed record GetSwimlanesByBoardIdQuery(int Id) : IQuery<IReadOnlyList<SwimlaneDto>>;