using Snapflow.Application.Users.Avatar;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Services;

public interface IAvatarService
{
    string GenerateAvatarUrl(int userId);

    Task<Result<AvatarResponse>> GetAvatarDataAsync(IUser user, CancellationToken cancellationToken);
}
