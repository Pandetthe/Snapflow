using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Boards.GetDetails;

public sealed record GetBoardDetailsResponse(
    int Id,
    string Title,
    string Description,
    BoardVisibility Visibility,
    IReadOnlyList<GetBoardDetailsMemberResponse> Members);

public sealed record GetBoardDetailsMemberResponse(
    int Id,
    string UserName,
    string? AvatarUrl,
    MemberRole Role);
