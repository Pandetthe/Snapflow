using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Snapflow.Infrastructure.Mailing;

internal sealed class EmailTemplateRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly EmailTemplateRegistry _registry;

    public EmailTemplateRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, EmailTemplateRegistry registry)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>
    /// Renderuje zarejestrowany szablon o podanej nazwie. Proste użycie: rejestrujesz szablony w EmailTemplateRegistry,
    /// potem wołasz RenderAsync("EmailConfirmation", model).
    /// </summary>
    public async Task<EmailRenderResult> RenderAsync<TModel>(string templateName, TModel model)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name cannot be null or empty.", nameof(templateName));
        if (!_registry.TryGet(templateName, out var componentType))
            throw new InvalidOperationException($"Template '{templateName}' is not registered in EmailTemplateRegistry.");

        string html = await RenderComponentToStringAsync(componentType, model, true).ConfigureAwait(false);
        string plain = await RenderComponentToStringAsync(componentType, model, false).ConfigureAwait(false);
        return new EmailRenderResult(html, plain);
    }

    private async Task<string> RenderComponentToStringAsync(Type componentType, object? model, bool isHtml)
    {
        var parameters = new Dictionary<string, object?> { ["Model"] = model, ["IsHtml"] = isHtml };
        var pv = ParameterView.FromDictionary(parameters);

        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);
        var result = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var rendered = await htmlRenderer.RenderComponentAsync(componentType, pv);
            return rendered.ToHtmlString();
        }).ConfigureAwait(false);

        return result ?? string.Empty;
    }
}