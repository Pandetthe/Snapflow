using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Create;

public sealed record CreateSwimlaneCommand(int BoardId, string Title, int? Height, int? BeforeId) : ICommand<int>;