using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Models;
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
        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                operation.Responses.TryAdd("500", new OpenApiResponse
                {
                    Description = ReasonPhrases.GetReasonPhrase(500),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ProblemDetails"
                                }
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
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowWeb", policy =>
            {
                string webAppUrl = builder.Configuration.GetValue<string>("Services:WebAppUrl") ?? "http://localhost:5173";
                policy.WithOrigins(webAppUrl)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });
        WebApplication app = builder.Build();

        app.UseCors("AllowWeb");

        app.MapEndpoints();
        app.MapHub<BoardHub>("/boards/{boardId:int}/hub");

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