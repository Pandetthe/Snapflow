using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Snapflow.Application.Abstractions.Identity;
using System.Net.Mail;

namespace Snapflow.Infrastructure.Mailing;

internal sealed class MailingProvider(
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor) : IEmailSender
{
    public async Task SendConfirmationLinkAsync(int userId, string email, string code)
    {
        if (httpContextAccessor.HttpContext is not { } httpContext)
            throw new InvalidOperationException("No http context available.");
        var routeValues = new RouteValueDictionary()
        {
            ["userId"] = userId,
            ["code"] = code,
        };
        var confirmEmailUrl = linkGenerator.GetUriByName(httpContext, "ConfirmEmail-auth/confirm-email", routeValues)
                ?? throw new NotSupportedException($"Could not find endpoint named 'ConfirmEmail-auth/confirm-email'.");
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
            Body = $"Please confirm your email by clicking on the following link: {confirmEmailUrl}",
        };
        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendPasswordResetCodeAsync(string email, string code)
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
            Subject = "Reset password code",
            Body = $"Here is password reset code: {code}",
        };
        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }
}
