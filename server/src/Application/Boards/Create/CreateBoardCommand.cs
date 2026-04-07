using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Boards.Create;

public sealed record CreateBoardMemberRequest(int UserId, MemberRole Role);

public sealed record CreateBoardCommand(
    string Title,
    string Description,
    IReadOnlyList<CreateBoardMemberRequest>? Members = null) : ICommand<int>;