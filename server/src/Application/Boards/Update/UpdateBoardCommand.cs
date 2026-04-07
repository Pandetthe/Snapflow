using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Boards.Update;

public sealed record UpdateBoardMemberRequest(int UserId, MemberRole Role);

public sealed record UpdateBoardCommand(
    int Id,
    string Title,
    string Description,
    IReadOnlyList<UpdateBoardMemberRequest>? Members = null) : ICommand;