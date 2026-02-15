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
    private readonly SmtpOptions _smtpOptions;
    private readonly EmailTemplateRenderer _templateRenderer;
    private readonly ServiceLinkBuilder _serviceLinkBuilder;

    public AuthEmailSender(
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        IOptions<SmtpOptions> smtpOptions,
        EmailTemplateRenderer templateRenderer,
        ServiceLinkBuilder serviceLinkBuilder)
    {
        _smtpOptions = smtpOptions.Value;
        _templateRenderer = templateRenderer;
        _serviceLinkBuilder = serviceLinkBuilder;
    }

    private async Task SendMailAsync(string toEmail, string subject, string bodyText, string bodyHtml)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, _smtpOptions.SecureSocketOptions);

        if (_smtpOptions.RequireAuthentication && !string.IsNullOrWhiteSpace(_smtpOptions.UserName))
            await client.AuthenticateAsync(_smtpOptions.UserName, _smtpOptions.Password);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
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