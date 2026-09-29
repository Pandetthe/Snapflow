using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;

namespace Snapflow.Presentation.Hubs.Board;

public sealed class BoardHubUsers(IAppDbContext dbContext, IAvatarService avatarService)
{
    public async Task<IBoardHubClient.UserDto?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        string? userName = await dbContext.FindUserNameAsync(userId, cancellationToken);
        return userName is null
            ? null
            : new IBoardHubClient.UserDto(userId, userName, avatarService.GenerateAvatarUrl(userId));
    }
}
