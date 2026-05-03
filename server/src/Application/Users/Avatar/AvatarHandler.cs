using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Avatar;

internal sealed class AvatarHandler(
    IUserManager userManager,
    IAvatarService avatarService) : IQueryHandler<AvatarQuery, AvatarResponse>
{
    public async Task<Result<AvatarResponse>> Handle(AvatarQuery query, CancellationToken cancellationToken)
    {
        IUser? user = await userManager.FindByIdAsync(query.UserId);

        if (user is null || user.IsDeleted)
            return Result.Failure<AvatarResponse>(UserErrors.NotFound(query.UserId));

        return await avatarService.GetAvatarDataAsync(user, cancellationToken);
    }
}
