using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    IUserManager userManager) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
            return Result.Success(); // TODO return invalid code;
        return await userManager.ResetPasswordAsync(user, command.ResetCode, command.NewPassword);
    }
}