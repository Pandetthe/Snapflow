using Scalar.AspNetCore;
using Snapflow.Application;
using Snapflow.Infrastructure;
using Snapflow.Presentation.Hubs.Board;

namespace Snapflow.Presentation;

public static class Program
{
    private static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddInfrastructure();

        builder.Services
            .AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddPresentation(builder.Configuration);
        
        WebApplication app = builder.Build();

        app.UseCors("AllowWeb");

        app.MapEndpoints();
        app.MapHub<BoardHub>("/boards/{boardId:int}/hub");

        app.MapDefaultEndpoints();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.ApplyMigrations();
        }
        else
        {
            app.UseHsts();
        }

        app.UseAuthentication();

        app.UseAuthorization();

        await app.RunAsync();
    }
}