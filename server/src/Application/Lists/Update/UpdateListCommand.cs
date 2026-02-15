using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Lists.Update;

public sealed record UpdateListCommand(int Id, string Title, int? Width) : ICommand;