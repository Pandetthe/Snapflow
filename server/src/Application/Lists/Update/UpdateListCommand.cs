using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Update;

public sealed record UpdateListCommand(int BoardId, int Id, string Title, int? Width) : ICommand<UpdateListResponse>;