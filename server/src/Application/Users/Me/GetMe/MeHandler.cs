using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.GetMe;

internal sealed class MeHandler(
    IUserContext userContext,
    IUserManager userManager,
    IAvatarService avatarService) : IQueryHandler<MeQuery, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(MeQuery query, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        bool emailConfirmed = await userManager.IsEmailConfirmedAsync(user);

        return new MeResponse(user.Id, user.UserName, user.Email, emailConfirmed, avatarService.GenerateAvatarUrl(user.Id), user.AvatarType);
    }
}
