using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Tags;

namespace Snapflow.Application.Tags.Create;

public sealed record CreateTagCommand(int BoardId, string Title, TagColors Color) : ICommand<CreateTagResponse>;
