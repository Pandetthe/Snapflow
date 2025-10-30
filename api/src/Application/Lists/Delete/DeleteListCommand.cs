using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Delete;

public sealed record DeleteListCommand(int Id, int BoardId, int SwimlaneId) : ICommand;