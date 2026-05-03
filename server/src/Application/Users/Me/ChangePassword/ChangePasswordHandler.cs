using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.ChangePassword;

internal sealed class ChangePasswordHandler(
    IUserContext userContext,
    IUserManager userManager) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
    }
}
