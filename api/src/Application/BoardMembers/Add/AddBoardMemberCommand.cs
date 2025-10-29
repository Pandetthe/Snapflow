using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Application.BoardMembers.Add;

public sealed record AddBoardMemberCommand(int UserId, int BoardId, BoardMemberRole Role) : ICommand<int>;