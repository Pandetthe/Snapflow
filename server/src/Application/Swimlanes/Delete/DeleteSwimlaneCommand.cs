using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Delete;

public sealed record DeleteSwimlaneCommand(int BoardId, int Id) : ICommand;