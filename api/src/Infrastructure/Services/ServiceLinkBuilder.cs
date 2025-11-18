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
        if (string.IsNullOrEmpty(options.Value.WebAppUrl))
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
            uriBuilder = new UriBuilder(options.Value.WebAppUrl)
            {
                Path = $"/reset-password",
            };
        }
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedResetCode = Uri.EscapeDataString(resetCode);
        uriBuilder.Query = $"email={encodedEmail}&resetCode={encodedResetCode}";
        return uriBuilder.Uri;
    }

    public Uri BuildEmailConfirmationRedirect()
    {
        UriBuilder uriBuilder;
        if (string.IsNullOrEmpty(options.Value.WebAppUrl))
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
            uriBuilder = new UriBuilder(options.Value.WebAppUrl)
            {
                Path = "/email-confirmed"
            };
        }
        return uriBuilder.Uri;
    }
}
