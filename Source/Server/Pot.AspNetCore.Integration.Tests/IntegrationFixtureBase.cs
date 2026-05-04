using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Shouldly;
using Testcontainers.PostgreSql;

namespace Pot.AspNetCore.Integration.Tests;

public abstract class IntegrationFixtureBase : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ProductionApiWebApplicationFactory? _factory;

    protected string DbHost { get; private set; }
    protected int DbPort { get; private set; }

    protected IServiceScope CreateScope()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        return _factory.Services.CreateScope();
    }

    protected HttpClient CreateClient(WebApplicationFactoryClientOptions? clientOptions = null)
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        return clientOptions is null
            ? _factory.CreateClient()
            : _factory.CreateClient(clientOptions);
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        // Step 1: Create a fresh isolated Postgres container for this fixture
        _container = new PostgreSqlBuilder("postgres:13")
            .WithDatabase(ApiWebApplicationFactory.TestDatabase)
            .WithUsername(ApiWebApplicationFactory.TestUsername)
            .WithPassword(ApiWebApplicationFactory.TestPassword)
            .Build();

        // Step 2: Start the container and get its connection details
        await _container.StartAsync();

        DbHost = _container.Hostname;
        DbPort = _container.GetMappedPublicPort(5432);

        // Step 3: Create the factory with ACTUAL container connection details
        // This ensures ConfigureWebHost() receives real values, not fallbacks
        _factory = new ProductionApiWebApplicationFactory(DbHost, DbPort);

        // Step 4: Apply EF Core migrations to the fresh isolated database
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _factory?.Dispose();
        _factory = null;

        if (_container is not null)
        {
            await _container.DisposeAsync();
            _container = null;
        }
    }
}
