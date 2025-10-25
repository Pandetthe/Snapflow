using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailCommandHandler(
    IUserManager userManager,
    IEmailSender emailSender)
    : ICommandHandler<ResendConfirmationEmailCommand>
{
    public async Task<Result> Handle(ResendConfirmationEmailCommand command, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not { } user)
            return Result.Success();

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailSender.SendConfirmationLinkAsync(user.Id, command.Email, code);
        return Result.Success();
    }
}