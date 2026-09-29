using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Snapflow.Application;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Create;
using Snapflow.Application.Lists.Create;
using Snapflow.Application.Swimlanes.Create;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Persistence;

namespace Snapflow.IntegrationTests;

public sealed class TestApp : IAsyncDisposable
{
    private TestApp(ServiceProvider services) => Services = services;

    public ServiceProvider Services { get; }

    public static TestApp Create(PostgresFixture fixture, Action<IServiceCollection>? configureServices = null)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = fixture.ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddLogging();
        services
            .AddApplication()
            .AddInfrastructure(configuration, new TestHostEnvironment());

        services.AddScoped<TestUserContext>();
        services.AddScoped<IUserContext>(provider => provider.GetRequiredService<TestUserContext>());

        configureServices?.Invoke(services);

        return new TestApp(services.BuildServiceProvider());
    }

    public AppDbContext CreateDbContext() =>
        Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

    public async Task<int> CreateUserAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = AppUser.Create($"user-{suffix}@example.com", $"user-{suffix}");

        await using AppDbContext db = CreateDbContext();
        db.Set<AppUser>().Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    public async Task<TestBoard> CreateBoardAsync()
    {
        int ownerId = await CreateUserAsync();
        int boardId = await SucceedAsync(SendAsync(ownerId, new CreateBoardCommand("Board", "")));
        int swimlaneId = (await SucceedAsync(SendAsync(ownerId, new CreateSwimlaneCommand(boardId, "Swimlane", null, null)))).Id;
        int listId = (await SucceedAsync(SendAsync(ownerId, new CreateListCommand(boardId, swimlaneId, "List", null, null)))).Id;
        return new TestBoard(ownerId, boardId, swimlaneId, listId);
    }

    public Task<Result> SendAsync(int userId, ICommand command) =>
        InvokeAsync<Result>(userId, typeof(ICommandHandler<>).MakeGenericType(command.GetType()), command);

    public Task<Result<TResponse>> SendAsync<TResponse>(int userId, ICommand<TResponse> command) =>
        InvokeAsync<Result<TResponse>>(
            userId, typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse)), command);

    public Task<Result<TResponse>> QueryAsync<TResponse>(int userId, IQuery<TResponse> query) =>
        InvokeAsync<Result<TResponse>>(
            userId, typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse)), query);

    public static async Task<TValue> SucceedAsync<TValue>(Task<Result<TValue>> pending)
    {
        Result<TValue> result = await pending;
        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Code : null);
        return result.Value;
    }

    public static async Task SucceedAsync(Task<Result> pending)
    {
        Result result = await pending;
        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Code : null);
    }

    private async Task<TResult> InvokeAsync<TResult>(int userId, Type handlerType, object request)
    {
        using IServiceScope scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TestUserContext>().UserId = userId;

        object handler = scope.ServiceProvider.GetRequiredService(handlerType);
        var pending = (Task<TResult>)handlerType.GetMethod("Handle")!
            .Invoke(handler, [request, CancellationToken.None])!;
        return await pending;
    }

    public ValueTask DisposeAsync() => Services.DisposeAsync();
}

public sealed record TestBoard(int OwnerId, int BoardId, int SwimlaneId, int ListId);

internal sealed class TestUserContext : IUserContext
{
    public int UserId { get; set; }

    public string UserName => $"user-{UserId}";

    public bool IsAuthenticated => true;

    public string? ConnectionId => null;

    public Task<IUser> GetUserAsync() => throw new NotSupportedException();
}

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;

    public string ApplicationName { get; set; } = "Snapflow.IntegrationTests";

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
