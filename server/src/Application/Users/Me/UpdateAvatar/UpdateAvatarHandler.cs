using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.UpdateAvatar;

internal sealed class UpdateAvatarHandler(
    IUserContext userContext,
    IUserManager userManager) : ICommandHandler<UpdateAvatarCommand>
{
    public async Task<Result> Handle(UpdateAvatarCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await userManager.UpdateAvatarAsync(user, command.AvatarType, command.AvatarData, command.ContentType);
    }
}
