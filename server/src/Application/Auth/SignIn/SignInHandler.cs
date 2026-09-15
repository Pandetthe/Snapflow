using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.SignIn;

internal sealed class SignInHandler(
    ISignInManager signInManager,
    IUserManager userManager)
    : ICommandHandler<SignInCommand>
{
    public async Task<Result> Handle(SignInCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result.Failure(UserErrors.SignInFailed);
        if (user.IsDeleted)
            return Result.Failure(UserErrors.AccountDeleted);
        return await signInManager.PasswordSignInAsync(user, command.Password, command.UseCookies, command.UseSessionCookies, true, command.RememberDeviceToken);
    }
}