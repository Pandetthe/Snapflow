using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.BoardMembers.ChangeOwner;

public sealed record ChangeBoardOwnerCommand(int UserId, int BoardId) : ICommand;