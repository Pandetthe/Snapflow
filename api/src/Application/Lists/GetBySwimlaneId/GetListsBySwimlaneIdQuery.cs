using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.GetBySwimlaneId;

public sealed record GetListsBySwimlaneIdQuery(int Id) : IQuery<ListsResponse>;