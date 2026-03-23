using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using MimeKit;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Common;
using Snapflow.Infrastructure.Mailing;
using Snapflow.Infrastructure.Mailing.Templates;

namespace Snapflow.Infrastructure.Identity.Services;

internal sealed class AuthEmailSender : IAuthEmailSender
{
    private readonly EmailOptions _emailOptions;
    private readonly EmailTemplateRenderer _templateRenderer;
    private readonly ServiceLinkBuilder _serviceLinkBuilder;

    public AuthEmailSender(
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        IOptions<EmailOptions> smtpOptions,
        EmailTemplateRenderer templateRenderer,
        ServiceLinkBuilder serviceLinkBuilder)
    {
        _emailOptions = smtpOptions.Value;
        _templateRenderer = templateRenderer;
        _serviceLinkBuilder = serviceLinkBuilder;
    }

    private async Task SendMailAsync(string toEmail, string subject, string bodyText, string bodyHtml)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, _emailOptions.SecureSocketOptions);

        if (_emailOptions.RequireAuthentication && !string.IsNullOrWhiteSpace(_emailOptions.UserName))
            await client.AuthenticateAsync(_emailOptions.UserName, _emailOptions.Password);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.FromEmail));
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

    public async Task SendConfirmationLinkAsync(IUser user, string code)
    {
        var model = new EmailConfirmationModel
        {
            UserName = user.UserName,
            ConfirmationLink = _serviceLinkBuilder.BuildEmailConfirmationLink(user.Email, code).ToString()
        };
        var result = await _templateRenderer.RenderAsync(EmailConfirmationModel.TemplateName, model);
        await SendMailAsync(user.Email, "Confirm your email", result.Plain, result.Html).ConfigureAwait(false);
    }

    public async Task SendPasswordResetLinkAsync(IUser user, string code)
    {
        var model = new PasswordResetModel
        {
            UserName = user.UserName,
            ResetFormLink = _serviceLinkBuilder.BuildPasswordResetLink(user.Email, code).ToString()
        };
        var result = await _templateRenderer.RenderAsync(PasswordResetModel.TemplateName, model);
        await SendMailAsync(user.Email, "Reset your password", result.Plain, result.Html).ConfigureAwait(false);
    }
}