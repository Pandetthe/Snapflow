using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MimeKit;
using Snapflow.Application.Abstractions.Identity;

namespace Snapflow.Infrastructure.Identity;

internal sealed class AuthEmailSender(
    IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator) : IAuthEmailSender
{
    public static async Task SendMailAsync(string toEmail, string subject, string bodyText, string bodyHtml)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync("localhost", 1025, false);
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress("No Reply", "noreply@snapflow.pl"));
                message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder
        {
            TextBody = bodyText,
            HtmlBody = bodyHtml,
        };
        message.Body = bodyBuilder.ToMessageBody();
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

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
        var subject = "Confirm your email";
        var bodyText = $"Please confirm your email by clicking the following link: {confirmEmailUrl}";
        var bodyHtml = $"<p>Please confirm your email by clicking the following link:</p><p><a href=\"{confirmEmailUrl}\">Confirmation</a></p>";
        await SendMailAsync(email, subject, bodyText, bodyHtml);
    }

    public async Task SendPasswordResetCodeAsync(string email, string code)
    {
        var subject = "Password reset code";
        var bodyText = $"Your password reset code is: {code}";
        var bodyHtml = $"<p>Your password reset code is:</p><h2>{code}</h2>";
        await SendMailAsync(email, subject, bodyText, bodyHtml);
    }
}
