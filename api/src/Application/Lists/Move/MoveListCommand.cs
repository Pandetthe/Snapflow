using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Move;

public sealed record MoveListCommand(int Id, int? BeforeId) : ICommand;