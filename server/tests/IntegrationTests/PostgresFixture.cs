using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snapflow.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Snapflow.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer? _container =
        DockerEndpoint.ExternalConnectionString is null
            ? new PostgreSqlBuilder("postgres:17-alpine").Build()
            : null;

    public string ConnectionString =>
        DockerEndpoint.ExternalConnectionString ?? _container!.GetConnectionString();

    public async Task InitializeAsync()
    {
        if (_container is not null)
            await _container.StartAsync();

        await using TestApp app = TestApp.Create(this);
        using IServiceScope scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
