using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Tags;

namespace Snapflow.Application.Tags.Update;

public sealed record UpdateTagCommand(int BoardId, int Id, string Title, TagColors Color) : ICommand<UpdateTagResponse>;
