using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Move;

public sealed record MoveSwimlaneCommand(int Id, int? BeforeId) : ICommand;