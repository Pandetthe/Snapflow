using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.Register;

internal sealed class RegisterCommandHandler(
    IUserManager userManager)
    : ICommandHandler<RegisterCommand>
{
    public async Task<Result> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var result = await userManager.CreateAsync(command.Email, command.UserName, command.Password);
        if (!result.IsSuccess)
            return result;
        var code = await userManager.GenerateEmailConfirmationTokenAsync(result.Value);
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