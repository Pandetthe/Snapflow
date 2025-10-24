using Microsoft.AspNetCore.Identity;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Identity;
using System.Net.Mail;

namespace Snapflow.Infrastructure.Mailing;

internal sealed class MailingProvider : IEmailSender<AppUser>
{
    public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        SmtpClient smtpClient = new SmtpClient("smtp.example.com")
        {
            Port = 1025,
            Host = "localhost",
            EnableSsl = false,
        };
        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress("noreply@snapflow.com"),
            Subject = "Confirm your email",
            Body = $"Please confirm your email by clicking on the following link: {confirmationLink}",
        };
        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        SmtpClient smtpClient = new SmtpClient("smtp.example.com")
        {
            Port = 1025,
            Host = "localhost",
            EnableSsl = false,
        };
        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress("noreply@snapflow.com"),
            Subject = "Reset password",
            Body = $"Please reset your password with the following code: {resetCode}",
        };
        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }
}
