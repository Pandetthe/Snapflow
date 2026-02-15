using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Add;

public sealed record AddMemberCommand(int UserId, int BoardId, MemberRole Role) : ICommand;