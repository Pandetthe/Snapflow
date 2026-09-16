namespace Snapflow.Application.Abstractions.Identity;

public interface IBoardMembershipService
{
    Task<bool> IsMemberAsync(int boardId, CancellationToken cancellationToken = default);
}
