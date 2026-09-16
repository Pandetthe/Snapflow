using Snapflow.Domain.Boards;

namespace Snapflow.Application.Abstractions.Services;

public interface IBoardVisibilityPolicy
{
    IReadOnlyList<BoardVisibility> AllowedVisibilities { get; }

    bool IsAllowed(BoardVisibility visibility);

    bool CanNonMemberView(BoardVisibility visibility, bool isAuthenticated);

    bool IsListed(BoardVisibility visibility);
}
