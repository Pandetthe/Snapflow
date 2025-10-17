using Snapflow.Application.Abstractions.Messaging;

namespace SnapflowCQRS;

public sealed record Example1Query : IQuery<Example1Response>;