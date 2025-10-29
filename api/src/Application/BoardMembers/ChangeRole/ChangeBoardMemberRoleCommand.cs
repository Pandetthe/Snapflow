using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Application.BoardMembers.ChangeRole;

public sealed record ChangeBoardMemberRoleCommand(int BoardId, int UserId, BoardMemberRole Role) : ICommand;