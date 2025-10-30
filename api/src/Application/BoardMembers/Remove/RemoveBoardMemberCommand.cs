using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.BoardMembers.Remove;

public sealed record RemoveBoardMemberCommand(int BoardId, int UserId) : ICommand;