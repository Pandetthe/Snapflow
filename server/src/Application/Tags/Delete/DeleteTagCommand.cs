using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Tags.Delete;

public sealed record DeleteTagCommand(int BoardId, int Id) : ICommand;
