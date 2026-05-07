using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Create;

public sealed record CreateListCommand(int BoardId, int SwimlaneId, string Title, int? Width, int? BeforeId) : ICommand<CreateListResponse>;