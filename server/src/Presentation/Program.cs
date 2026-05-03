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
            .AddInfrastructure(builder.Configuration, builder.Environment)
            .AddPresentation(builder.Configuration);
        
        WebApplication app = builder.Build();

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

        app.UseCors("AllowWeb");

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseOutputCache();

        app.MapEndpoints();
        app.MapHub<BoardHub>("/boards/{boardId:int}/hub");

        app.MapDefaultEndpoints();

        await app.RunAsync();
    }
}
