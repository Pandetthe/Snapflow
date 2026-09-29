using Snapflow.Application.Abstractions.Messaging;

namespace SnapflowCQRS;

public sealed record Example1Command(int BoardId, string Name) : ICommand;
