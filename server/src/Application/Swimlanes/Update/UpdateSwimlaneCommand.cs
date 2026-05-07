using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Swimlanes.Update;

public sealed record UpdateSwimlaneCommand(int BoardId, int Id, string Title, int? Height) : ICommand<UpdateSwimlaneResponse>;