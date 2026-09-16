using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.ChangeVisibility;

public sealed record ChangeBoardVisibilityCommand(int Id, BoardVisibility Visibility) : ICommand;
