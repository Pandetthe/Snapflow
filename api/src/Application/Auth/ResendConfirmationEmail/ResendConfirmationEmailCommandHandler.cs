using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using System.Text;

namespace Snapflow.Application.Auth.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailCommandHandler(
    IUserManager userManager)
    : ICommandHandler<ResendConfirmationEmailCommand>
{
    public async Task<Result> Handle(ResendConfirmationEmailCommand command, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not { } user)
            return Result.Success();

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //var userId = await userManager.GetUserIdAsync(user);
        //var routeValues = new RouteValueDictionary()
        //{
        //    ["userId"] = userId,
        //    ["code"] = code,
        //};
        //var confirmEmailUrl = linkGenerator.GetUriByName(command.Context, "ConfirmEmail-auth/confirm-email", routeValues)
        //        ?? throw new NotSupportedException($"Could not find endpoint named 'ConfirmEmail-auth/confirm-email'.");
        //await emailSender.SendConfirmationLinkAsync(user, command.Email, confirmEmailUrl);
        return Result.Success();
    }
}