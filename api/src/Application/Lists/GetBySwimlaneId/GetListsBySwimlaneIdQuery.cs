using Snapflow.Application.Abstractions.Messaging;
using static Snapflow.Application.Lists.GetBySwimlaneId.GetListsBySwimlaneIdResponse;

namespace Snapflow.Application.Lists.GetBySwimlaneId;

public sealed record GetListsBySwimlaneIdQuery(int Id) : IQuery<IReadOnlyList<ListDto>>;