using Snapflow.Application.Abstractions.Messaging;

namespace SnapflowCQRS;

public sealed record Example1Query(int BoardId) : IQuery<Example1Response>;
