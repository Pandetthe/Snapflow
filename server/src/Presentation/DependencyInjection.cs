using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using Snapflow.Common;
using Snapflow.Infrastructure.Persistence;
using Snapflow.Presentation.Endpoints;
using Snapflow.Presentation.Middlewares;
using StackExchange.Redis;
using System.Reflection;

namespace Snapflow.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsInternal();
        services.AddEndpointsApiExplorer();

        services.AddDomainEventHandlersInternal();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        var signalRBuilder = services.AddSignalR(options =>
        {
            options.AddFilter<GlobalHubExceptionFilter>();
            options.AddFilter<ConnectionIdHubFilter>();
        });

        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn))
        {
            signalRBuilder.AddStackExchangeRedis(redisConn, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("Snapflow_SignalR");
            });
        }

        services.AddProblemDetails();

        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                operation.Responses ??= new OpenApiResponses();
                operation.Responses.TryAdd("500", new OpenApiResponse
                {
                    Description = ReasonPhrases.GetReasonPhrase(500),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Title = "ProblemDetails"
                            }
                        }
                    }
                });
                return Task.CompletedTask;
            });
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                var type = context.JsonTypeInfo.Type;

                if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                {
                    return Task.CompletedTask;
                }
                var isMyAssembly = type.Assembly == typeof(Application.DependencyInjection).Assembly;

                if (isMyAssembly)
                {
                    if (type.FullName != null)
                    {
                        var simpleName = type.Name;

                        if (type.IsNested)
                        {
                            schema.Title = type.FullName.Split('.').Last().Replace('+', '.');
                        }
                        else
                        {
                            schema.Title = type.Name;
                        }
                    }
                }
                return Task.CompletedTask;
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowWeb", policy =>
            {
                var webUrl = configuration.GetValue<string>("Services:WebUrl");
                if (!string.IsNullOrEmpty(webUrl))
                {
                    policy.WithOrigins(webUrl)
                          .SetIsOriginAllowedToAllowWildcardSubdomains()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });
        return services;
    }

    private static IServiceCollection AddEndpointsInternal(this IServiceCollection services)
    {
        ServiceDescriptor[] serviceDescriptors = Assembly
            .GetExecutingAssembly()
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    private static IServiceCollection AddDomainEventHandlersInternal(this IServiceCollection services)
    {
        return services
            .Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        var exposeFullChecks = app.Configuration.GetValue<bool>("ExposeHealthChecks");

        if (app.Environment.IsDevelopment() || exposeFullChecks)
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();
        }

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
        }).AllowAnonymous();

        return app;
    }

    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationRunner>>();

        try
        {
            logger.LogInformation("Applying migrations...");
            using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while applying migrations.");
            throw;
        }
    }

    private sealed class MigrationRunner { }
}
