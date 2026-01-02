using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Services;

// This endpoint serves a fallback HTML form for email confirmation
// in development environments where web application is nor running separately.

namespace Snapflow.Api.Endpoints.Auth.FallbackForms;

internal sealed class ResetPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<ServicesOptions>>();
        if (!string.IsNullOrEmpty(options.Value.WebUrl))
            return;

        app.MapGet("auth/reset-password", async (
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            string email,
            string resetCode,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(resetCode) || string.IsNullOrEmpty(email))
                return Results.NotFound();
            await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

            var rendered = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                ParameterView parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    { "ResetCode", resetCode },
                    { "Email", email },
                    { "MinPasswordLength", UserOptions.MinPasswordLength },
                    { "MaxPasswordLength", UserOptions.MaxPasswordLength },
                    { "RequireNonAlphanumericInPassword", UserOptions.RequireNonAlphanumericInPassword },
                    { "RequireDigitInPassword", UserOptions.RequireDigitInPassword },
                    { "RequireLowercaseInPassword", UserOptions.RequireLowercaseInPassword },
                    { "RequireUppercaseInPassword", UserOptions.RequireUppercaseInPassword }
                });
                var result = await htmlRenderer.RenderComponentAsync<ResetPasswordView>(parameters);
                return result.ToHtmlString();
            });
            return Results.Content(rendered, "text/html; charset=utf-8");
        })
        .WithTags(EndpointTags.Auth);
    }
}