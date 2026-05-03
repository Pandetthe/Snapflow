using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface IAuthEmailSender
{
    Task SendPasswordResetLinkAsync(IUser user, string code);

    Task SendConfirmationLinkAsync(IUser user, string code);

    Task SendEmailChangeLinkAsync(IUser user, string newEmail, string code);
}
