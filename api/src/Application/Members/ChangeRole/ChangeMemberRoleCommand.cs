using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.ChangeRole;

public sealed record ChangeMemberRoleCommand(int BoardId, int UserId, MemberRole Role) : ICommand;