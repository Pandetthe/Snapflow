using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Presentation.Endpoints.Auth.FallbackForms;

// This endpoint serves a fallback HTML form for email confirmation
// in development environments where web application is nor running separately.

internal sealed class EmailConfirmed : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<ServicesOptions>>();
        if (!string.IsNullOrEmpty(options.Value.WebUrl))
            return;

        app.MapGet("auth/email-confirmed", async (
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

            var rendered = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var result = await htmlRenderer.RenderComponentAsync<EmailConfirmedView>();
                return result.ToHtmlString();
            });
            return Results.Content(rendered, "text/html; charset=utf-8");
        })
        .WithTags(EndpointTags.Auth);
    }
}