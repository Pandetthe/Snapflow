using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ConfirmEmail;

internal sealed class ConfirmEmailCommandHandler(
    IUserManager userManager) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<Result> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not { } user)
            return Result.Failure(UserErrors.NotFoundByEmail);

        if (string.IsNullOrEmpty(command.ChangedEmail))
            return await userManager.ConfirmEmailAsync(user, command.Code);
        return await userManager.ChangeEmailAsync(user, command.ChangedEmail, command.Code);
    }
}