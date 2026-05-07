using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.GetById;

public sealed record GetListByIdQuery(int BoardId, int Id) : IQuery<GetListByIdResponse>;