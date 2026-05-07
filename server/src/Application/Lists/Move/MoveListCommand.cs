using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Move;

public sealed record MoveListCommand(int BoardId, int Id, int SwimlaneId, int? BeforeId) : ICommand<string>;