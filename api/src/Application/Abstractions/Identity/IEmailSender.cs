using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface IEmailSender
{
    Task SendPasswordResetCodeAsync(IUser user, string email, string code);
}
