using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Replace;

public sealed record ReplaceMemberRequest(int UserId, MemberRole Role);

public sealed record ReplaceMembersCommand(int BoardId, IReadOnlyList<ReplaceMemberRequest> Members) : ICommand;