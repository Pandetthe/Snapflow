namespace Snapflow.Application.Abstractions.Identity;

public interface IBoardPermissionService
{
    Task<bool> HasPermissionAsync(int boardId, string permission, CancellationToken cancellationToken = default);
}
