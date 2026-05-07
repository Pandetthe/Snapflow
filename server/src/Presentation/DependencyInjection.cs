using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Common;
using Snapflow.Infrastructure.Persistence;
using Snapflow.Presentation.Caching;
using Snapflow.Presentation.Common;
using Snapflow.Presentation.Endpoints;
using Snapflow.Presentation.Middlewares;
using StackExchange.Redis;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snapflow.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsInternal();
        services.AddEndpointsApiExplorer();

        services.AddDomainEventHandlersInternal();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            );
        });
        
        ISignalRServerBuilder signalRBuilder = services.AddSignalR(options =>
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

            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = "Snapflow_OutputCache_";
            });
        }

        services.AddProblemDetails();

        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer((operation, _, _) =>
            {
                operation.Responses ??= new OpenApiResponses();
                operation.Responses.TryAdd("500", new OpenApiResponse
                {
                    Description = ReasonPhrases.GetReasonPhrase(500),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new()
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
            options.AddSchemaTransformer((schema, context, _) =>
            {
                Type type = context.JsonTypeInfo.Type;

                if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                {
                    return Task.CompletedTask;
                }
                var isMyAssembly = type.Assembly == typeof(Application.DependencyInjection).Assembly;

                if (!isMyAssembly)
                {
                    return Task.CompletedTask;
                }

                if (type.FullName != null)
                {
                    schema.Title = type.IsNested ? type.FullName.Split('.').Last().Replace('+', '.') : type.Name;
                }
                return Task.CompletedTask;
            });
        });

        services.AddSingleton<UserOutputCachePolicy>();
        services.AddSingleton<BoardOutputCachePolicy>();
        services.AddSingleton<AvatarOutputCachePolicy>();

        services.AddOutputCache(options =>
        {
            options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
            options.AddPolicy(CachePolicies.User, b => b.AddPolicy<UserOutputCachePolicy>());
            options.AddPolicy(CachePolicies.Board, b => b.AddPolicy<BoardOutputCachePolicy>());
            options.AddPolicy(CachePolicies.Avatar, b => b.AddPolicy<AvatarOutputCachePolicy>().Expire(TimeSpan.FromMinutes(5)));
        });

        var servicesOptions = configuration.GetSection(ServicesOptions.SectionName).Get<ServicesOptions>() ?? new ServicesOptions();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowWeb", policy =>
            {
                if (servicesOptions.AllowedOrigins is { Length: > 0 })
                {
                    policy.WithOrigins(servicesOptions.AllowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });

        if (servicesOptions.AllowedOrigins is not { Length: > 0 })
        {
            services.AddSingleton<IStartupFilter, CorsWarningStartupFilter>();
        }
        return services;
    }

    private static IServiceCollection AddEndpointsInternal(this IServiceCollection services)
    {
        var serviceDescriptors = Assembly
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
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

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
