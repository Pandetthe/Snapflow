using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Update;

public sealed record UpdateSwimlaneCommand(int Id, string Title) : ICommand;