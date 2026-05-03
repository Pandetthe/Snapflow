using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.UpdateProfile;

internal sealed class UpdateProfileHandler(
    IUserContext userContext,
    IUserManager userManager) : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await userManager.UpdateUserNameAsync(user, command.UserName);
    }
}
