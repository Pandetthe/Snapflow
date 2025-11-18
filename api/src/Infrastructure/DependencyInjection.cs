using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Infrastructure.Authorization;
using Snapflow.Infrastructure.Behaviours;
using Snapflow.Infrastructure.DomainEvents;
using Snapflow.Infrastructure.Identity;
using Snapflow.Infrastructure.Identity.Entities;
using Snapflow.Infrastructure.Mailing;
using Snapflow.Infrastructure.Mailing.Templates;
using Snapflow.Infrastructure.Persistence;
using Snapflow.Infrastructure.Services;

namespace Snapflow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddIdentityInternal()
            .AddServices()
            .AddAuthorizationInternal()
            .AddHttpContextAccessor();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, AppUserContext>();
        services.AddScoped<IRankService, LexoRankService>();
        services.AddSingleton(sp =>
        {
            var registry = new EmailTemplateRegistry();
            registry.Register<EmailConfirmation>(EmailConfirmationModel.TemplateName);
            registry.Register<PasswordReset>(PasswordResetModel.TemplateName);
            return registry;
        });
        services.AddScoped<EmailTemplateRenderer>();
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<ServicesOptions>(configuration.GetSection("Services"));
        services.AddScoped<ServiceLinkBuilder>();
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<IAppDbContext, AppDbContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("migration_history", Schemas.Default);
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                })
                .UseSnakeCaseNamingConvention());
        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }

    private static IServiceCollection AddIdentityInternal(this IServiceCollection services)
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
        });
        services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
        services.AddAuthorizationBuilder();
        services.AddScoped<IUserManager, AppUserManager>();
        services.AddScoped<ISignInManager, AppSignInManager>();
        services.AddScoped<IRefreshTokenValidator, AppRefreshTokenValidator>();
        services.AddScoped<IUserContext, AppUserContext>();
        services.AddScoped<IAuthEmailSender, AuthEmailSender>();
        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, UserPermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, BoardPermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }
}
