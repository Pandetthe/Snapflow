using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Delete;

public sealed record DeleteSwimlaneCommand(int Id) : ICommand;