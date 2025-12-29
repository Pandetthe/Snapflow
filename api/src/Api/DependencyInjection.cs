using Microsoft.AspNetCore.SignalR;
using Snapflow.Api.Infrastructure.Exceptions;
using Snapflow.Common;

namespace Snapflow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSignalR(options =>
        {
            options.AddFilter<GlobalHubExceptionFilter>();
        });

        services.AddProblemDetails();

        return services;
    }
}
