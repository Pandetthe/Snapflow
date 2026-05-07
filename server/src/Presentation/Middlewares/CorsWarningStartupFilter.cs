namespace Snapflow.Presentation.Middlewares;

internal sealed class CorsWarningStartupFilter(ILogger<CorsWarningStartupFilter> logger) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        logger.LogWarning("No AllowedOrigins configured — all cross-origin requests will be blocked.");
        return next;
    }
}
