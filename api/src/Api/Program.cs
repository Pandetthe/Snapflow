using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Snapflow.Api.Extensions;
using Snapflow.Api.Hubs.Board;
using Snapflow.Application;
using Snapflow.Infrastructure;
using System.Reflection;

namespace Snapflow.Api;

public static class Program
{
    private static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services
            .AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddPresentation();

        builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
        builder.Services.AddOpenApi();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
        WebApplication app = builder.Build();

        app.UseCors("AllowAll");

        app.MapEndpoints();
        app.MapHub<BoardHub>("/boards/hub");

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.ApplyMigrations();
        }

        app.MapHealthChecks("health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseExceptionHandler();

        app.UseAuthentication();

        app.UseAuthorization();

        await app.RunAsync();
    }
}
