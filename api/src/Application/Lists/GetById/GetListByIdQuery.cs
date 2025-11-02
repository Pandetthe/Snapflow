using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.GetById;

public sealed record GetListByIdQuery(int Id) : IQuery<ListResponse>;