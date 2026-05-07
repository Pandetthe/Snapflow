using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Delete;

public sealed record DeleteListCommand(int BoardId, int Id) : ICommand;