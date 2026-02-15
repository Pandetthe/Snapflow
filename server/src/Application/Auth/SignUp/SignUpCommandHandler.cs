using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.SignUp;

internal sealed class SignUpCommandHandler(
    IUserManager userManager,
    IAuthEmailSender emailSender)
    : ICommandHandler<SignUpCommand>
{
    public async Task<Result> Handle(SignUpCommand command, CancellationToken cancellationToken = default)
    {
        var result = await userManager.CreateAsync(command.Email, command.UserName, command.Password);
        if (!result.IsSuccess)
            return result;
        var code = await userManager.GenerateEmailConfirmationTokenAsync(result.Value);
        await emailSender.SendConfirmationLinkAsync(result.Value, code);
        return Result.Success();
    }
}