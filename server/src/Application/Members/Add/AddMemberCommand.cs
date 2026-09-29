using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Add;

public sealed record AddMemberCommand(int BoardId, int UserId, MemberRole Role) : ICommand;