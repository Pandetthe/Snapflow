using Microsoft.AspNetCore.SignalR;
using Snapflow.Api.Infrastructure.Exceptions;

namespace Snapflow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSignalR(options =>
        {
            options.AddFilter<GlobalHubExceptionFilter>();
        });

        services.AddProblemDetails();

        return services;
    }
}
