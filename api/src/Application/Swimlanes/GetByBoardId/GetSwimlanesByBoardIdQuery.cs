using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

public sealed record GetSwimlanesByBoardIdQuery(int Id) : IQuery<SwimlanesResponse>;