using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Move;

public sealed record MoveSwimlaneCommand(int BoardId, int Id, int? BeforeId) : ICommand<string>;