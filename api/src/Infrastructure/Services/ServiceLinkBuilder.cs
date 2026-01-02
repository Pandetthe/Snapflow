using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Snapflow.Infrastructure.Services;

public sealed class ServiceLinkBuilder(
    IOptions<ServicesOptions> options,
    IHttpContextAccessor httpContextAccessor)
{
    public Uri BuildPasswordResetLink(string email, string resetCode)
    {
        UriBuilder uriBuilder;
        if (string.IsNullOrEmpty(options.Value.WebUrl))
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context.");
            string scheme = httpContext.Request.Scheme;
            var requestHost = httpContext.Request.Host;
            string host = requestHost.Host;
            int? port = requestHost.Port;
            uriBuilder = new UriBuilder
            {
                Scheme = scheme,
                Host = host,
                Port = port ?? -1,
                Path = $"/auth/reset-password",
            };
        }
        else
        {
            uriBuilder = new UriBuilder(options.Value.WebUrl);
            uriBuilder.Path = $"{uriBuilder.Path.TrimEnd('/')}/reset-password";
        }
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedResetCode = Uri.EscapeDataString(resetCode);
        uriBuilder.Query = $"email={encodedEmail}&resetCode={encodedResetCode}";
        return uriBuilder.Uri;
    }

    public Uri BuildEmailConfirmationLink(string email, string code)
    {
        UriBuilder uriBuilder;
        if (string.IsNullOrEmpty(options.Value.ApiUrl))
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context.");
            string scheme = httpContext.Request.Scheme;
            var requestHost = httpContext.Request.Host;
            string host = requestHost.Host;
            int? port = requestHost.Port;
            uriBuilder = new UriBuilder
            {
                Scheme = scheme,
                Host = host,
                Port = port ?? -1,
                Path = $"/auth/confirm-email",
            };
        }
        else
        {
            uriBuilder = new UriBuilder(options.Value.ApiUrl);
            uriBuilder.Path = $"{uriBuilder.Path.TrimEnd('/')}/auth/confirm-email";
        }
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedCode = Uri.EscapeDataString(code);
        uriBuilder.Query = $"email={encodedEmail}&code={encodedCode}";
        return uriBuilder.Uri;
    }

    public Uri BuildEmailConfirmationRedirect()
    {
        UriBuilder uriBuilder;
        if (string.IsNullOrEmpty(options.Value.WebUrl))
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context.");
            string scheme = httpContext.Request.Scheme;
            var requestHost = httpContext.Request.Host;
            string host = requestHost.Host;
            int port = requestHost.Port.GetValueOrDefault();
            uriBuilder = new UriBuilder
            {
                Scheme = scheme,
                Host = host,
                Port = port,
                Path = "/auth/email-confirmed"
            };
        }
        else
        {
            uriBuilder = new UriBuilder(options.Value.WebUrl);
            uriBuilder.Path = $"{uriBuilder.Path.TrimEnd('/')}/email-confirmed";
        }
        return uriBuilder.Uri;
    }
}
