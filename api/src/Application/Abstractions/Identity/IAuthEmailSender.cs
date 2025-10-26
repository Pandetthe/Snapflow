namespace Snapflow.Application.Abstractions.Identity;

public interface IAuthEmailSender
{
    Task SendPasswordResetCodeAsync(string email, string code);

    Task SendConfirmationLinkAsync(int userId, string email, string code);
}
