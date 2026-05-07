using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.GetById;

public sealed record GetSwimlaneByIdQuery(int BoardId, int Id) : IQuery<GetSwimlaneByIdResponse>;