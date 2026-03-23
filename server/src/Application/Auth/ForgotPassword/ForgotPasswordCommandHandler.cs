using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler(
    IUserManager userManager,
    IAuthEmailSender emailSender) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await userManager.FindByEmailAsync(command.Email);

        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
            return Result.Success();

        var code = await userManager.GeneratePasswordResetTokenAsync(user);

        await emailSender.SendPasswordResetLinkAsync(user, code);

        // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we had
        // returned a 400 for an invalid code given a valid user email.
        return Result.Success();
    }
}