using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.SetPassword;

internal sealed class SetPasswordHandler(
    IUserContext userContext,
    IUserManager userManager,
    ISignInManager signInManager) : ICommandHandler<SetPasswordCommand>
{
    public async Task<Result> Handle(SetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        if (await userManager.HasPasswordAsync(user))
            return Result.Failure(UserErrors.PasswordAlreadySet);

        Result added = await userManager.AddPasswordAsync(user, command.NewPassword);
        if (added.IsFailure)
            return added;

        await signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }
}
