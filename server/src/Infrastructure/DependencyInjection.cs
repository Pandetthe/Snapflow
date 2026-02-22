using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Infrastructure.Auth.Accessors;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Auth.Managers;
using Snapflow.Infrastructure.Authorization;
using Snapflow.Infrastructure.Common;
using Snapflow.Infrastructure.Identity.Entities;
using Snapflow.Infrastructure.Identity.Services;
using Snapflow.Infrastructure.Mailing;
using Snapflow.Infrastructure.Mailing.Templates;
using Snapflow.Infrastructure.Persistence;
using Snapflow.Infrastructure.Persistence.Interceptors;

namespace Snapflow.Infrastructure;

public static class DependencyInjection
{
    public static ILoggingBuilder AddInfrastructure(this ILoggingBuilder logging)
    {
        logging.AddOpenTelemetry(loggingOptions =>
        {
            loggingOptions.IncludeFormattedMessage = true;
            loggingOptions.IncludeScopes = true;
        });
        return logging;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddServiceDiscovery();
        services.AddHttpContextAccessor();
        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        services.AddPostgresInternal(configuration);
        services.AddRedisCacheInternal(configuration);
        services.AddHealthChecksInternal(configuration);
        services.AddOpenTelemetryInternal(configuration);
        services.AddAuthInternal();
        services.AddMailingInternal(configuration);

        services.AddScoped<DomainEventsBuffer>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.AddScoped<IRankService, LexoRankService>();
        services.AddSingleton<HubCallerContextAccessor>();
        services.AddScoped<ServiceLinkBuilder>();
        return services;
    }

    private static IServiceCollection AddPostgresInternal(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Postgres")
                    ?? throw new InvalidOperationException("Postgres connection string is missing.");
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddDbContext<IAppDbContext, AppDbContext>(
            (sp, options) => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("migration_history", Schemas.Default);
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(5);
                })
                .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>())
                .UseSnakeCaseNamingConvention());
        return services;
    }

    private static IServiceCollection AddRedisCacheInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConn = configuration.GetConnectionString("Redis");

        // Redis is an optional dependency, so if the connection string is not provided, we simply skip configuring the cache.
        if (string.IsNullOrEmpty(redisConn)) return services;

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "Snapflow_Cache_";
        });

        return services;
    }

    private static IServiceCollection AddHealthChecksInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var healthBuilder = services.AddHealthChecks();

        healthBuilder.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        var postgresConn = configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrEmpty(postgresConn))
        {
            healthBuilder.AddNpgSql(
                connectionString: postgresConn,
                name: "postgres",
                tags: ["ready"]);
        }

        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn))
        {
            healthBuilder.AddRedis(
                redisConnectionString: redisConn,
                name: "redis",
                tags: ["ready"]);
        }
        return services;
    }

    private static IServiceCollection AddOpenTelemetryInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var otelBuilder = services.AddOpenTelemetry();

        otelBuilder.ConfigureResource(resource => resource
            .AddService(serviceName: configuration["OTEL_SERVICE_NAME"] ?? "api-server"));

        if (!string.IsNullOrEmpty(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            otelBuilder.UseAzureMonitor();
        }

        if (!string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            otelBuilder.UseOtlpExporter();
        }

        otelBuilder
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("Npgsql");
            })
            .WithTracing(tracing =>
            {
                if (string.Equals(configuration["OTEL_SAMPLER"], "always_on", StringComparison.OrdinalIgnoreCase))
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health") &&
                        !context.Request.Path.StartsWithSegments("/alive");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddNpgsql()
                .AddRedisInstrumentation();
            });

        return services;
    }

    private static IServiceCollection AddAuthInternal(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = Domain.Users.UserOptions.MinPasswordLength;
            options.Password.RequireLowercase = Domain.Users.UserOptions.RequireLowercaseInPassword;
            options.Password.RequireUppercase = Domain.Users.UserOptions.RequireUppercaseInPassword;
            options.Password.RequireDigit = Domain.Users.UserOptions.RequireDigitInPassword;
            options.Password.RequireNonAlphanumeric = Domain.Users.UserOptions.RequireNonAlphanumericInPassword;
        })
                .AddSignInManager()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Snapflow.Auth.Cookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
        services.AddAuthorizationBuilder();
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();
        services.AddTransient<IAuthorizationHandler, BoardPermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        services.AddScoped<IUserManager, AppUserManager>();
        services.AddScoped<ISignInManager, AppSignInManager>();
        services.AddScoped<IRefreshTokenValidator, AppRefreshTokenValidator>();
        services.AddScoped<IUserContext, AppUserContext>();
        services.AddScoped<IAuthEmailSender, AuthEmailSender>();

        return services;
    }

    private static IServiceCollection AddMailingInternal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var registry = new EmailTemplateRegistry();
            registry.Register<EmailConfirmation>(EmailConfirmationModel.TemplateName);
            registry.Register<PasswordReset>(PasswordResetModel.TemplateName);
            return registry;
        });

        services.AddScoped<EmailTemplateRenderer>();
        services.Configure<SmtpOptions>(configuration.GetSection("Email"));
        services.Configure<ServicesOptions>(configuration.GetSection("Services"));

        return services;
    }
}
