using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.RequestEmailChange;

internal sealed class RequestEmailChangeHandler(
    IUserContext userContext,
    IUserManager userManager,
    IAuthEmailSender emailSender) : ICommandHandler<RequestEmailChangeCommand>
{
    public async Task<Result> Handle(RequestEmailChangeCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        if (user.Email.Equals(command.NewEmail, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(UserErrors.EmailSameAsCurrent);

        var token = await userManager.GenerateEmailChangeTokenAsync(user, command.NewEmail);
        await emailSender.SendEmailChangeLinkAsync(user, command.NewEmail, token);
        return Result.Success();
    }
}
