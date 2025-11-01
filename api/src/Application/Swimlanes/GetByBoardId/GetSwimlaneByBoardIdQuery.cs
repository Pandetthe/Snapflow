using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

public sealed record GetSwimlaneByBoardIdQuery(int Id) : IQuery<List<GetSwimlaneByBoardIdResponse>>;