using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me;

internal sealed class MeHandler(
    IUserContext userContext,
    IAvatarService avatarService) : IQueryHandler<MeQuery, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(MeQuery query, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        
        var finalAvatarUrl = user.AvatarType switch
        {
            AvatarType.Gravatar => avatarService.GetGravatarUrl(user.Email),
            _ => avatarService.GenerateAvatarUrl(user.Id)
        };
        
        return new MeResponse(user.Id, user.UserName, user.Email, finalAvatarUrl, user.AvatarType);
    }
}