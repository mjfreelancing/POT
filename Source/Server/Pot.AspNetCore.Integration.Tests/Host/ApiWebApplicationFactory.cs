using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Base factory for creating test web application instances with isolated Postgres databases.
///
/// ## Initialization Pattern
///
/// This factory is INTENTIONALLY SYNCHRONOUS and does NOT implement IAsyncLifetime. Container lifecycle
/// management is delegated to test fixtures to ensure proper initialization order:
///
/// PROBLEM (Anti-pattern):
/// - WebApplicationFactory is constructed by xUnit before IAsyncLifetime.InitializeAsync() is called
/// - ConfigureWebHost() is invoked during factory construction, before async initialization
/// - This means _container is always null when ConfigureWebHost() runs, forcing fallback to localhost
/// - Fallbacks mask the fact that TestContainers is never actually used
/// - Tests are not isolated and may interfere with local Postgres or other test fixtures
///
/// SOLUTION (This Pattern):
/// 1. Test fixture (e.g., LoginFixture) implements IAsyncLifetime
/// 2. InitializeAsync() creates and starts a fresh PostgreSqlContainer
/// 3. InitializeAsync() creates ApiWebApplicationFactory with ACTUAL container connection details
/// 4. At this point, ConfigureWebHost() runs with populated _dbHost and _dbPort
/// 5. Migrations are applied to the fresh database
/// 6. DisposeAsync() stops the container, ensuring true cleanup and isolation
///
/// This ensures:
/// - Each fixture gets a dedicated, isolated database container
/// - No connection fallbacks (no localhost fallback hides initialization bugs)
/// - Tests are deterministic and non-interfering
/// - TestContainers is actually being used as intended
///
/// ## Usage Example:
///
/// public class LoginFixture : IAsyncLifetime
/// {
///     private PostgreSqlContainer _container = null!;
///     private ApiWebApplicationFactory _factory = null!;
///
///     async Task IAsyncLifetime.InitializeAsync()
///     {
///         _container = new PostgreSqlBuilder("postgres:13")
///             .WithDatabase("pot_test")
///             .WithUsername("postgres")
///             .WithPassword("postgres")
///             .Build();
///
///         await _container.StartAsync();
///
///         // NOW create factory with real container connection details
///         _factory = new ProductionApiWebApplicationFactory(
///             _container.Hostname,
///             _container.GetMappedPublicPort(5432));
///
///         // Apply migrations
///         using var scope = _factory.Services.CreateScope();
///         var dbContext = scope.ServiceProvider.GetRequiredService&lt;PotDbContext&gt;();
///         await dbContext.Database.MigrateAsync();
///     }
///
///     async Task IAsyncLifetime.DisposeAsync()
///     {
///         if (_factory is not null) _factory.Dispose();
///         if (_container is not null) await _container.DisposeAsync();
///     }
/// }
/// </summary>
public abstract class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestDatabase = "pot_test";
    public const string TestUsername = "postgres";
    public const string TestPassword = "postgres";

    /// <summary>
    /// Database hostname (set by constructor, passed from container at initialization time).
    /// </summary>
    private readonly string _dbHost;

    /// <summary>
    /// Database port (set by constructor, passed from container at initialization time).
    /// </summary>
    private readonly int _dbPort;

    /// <summary>
    /// Initializes factory with connection details for an already-running database.
    /// This design ensures ConfigureWebHost() has real values during factory construction.
    /// </summary>
    /// <param name="dbHost">Database hostname (e.g., "localhost" or container Hostname)</param>
    /// <param name="dbPort">Database port (e.g., 5432 or container GetMappedPublicPort(5432))</param>
    protected ApiWebApplicationFactory(string dbHost, int dbPort)
    {
        _dbHost = dbHost ?? throw new ArgumentNullException(nameof(dbHost));
        _dbPort = dbPort > 0 ? dbPort : throw new ArgumentOutOfRangeException(nameof(dbPort), "Port must be greater than 0");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins"] = "http://localhost:3000,https://www.localhost:3000",
                ["Database:Name"] = TestDatabase,
                ["Database:Host"] = _dbHost,
                ["Database:Username"] = TestUsername,
                ["Database:Password"] = TestPassword,
                ["Database:Port"] = _dbPort.ToString(),
                ["Database:SSLMode"] = "Disable",
                ["Jwt:Issuer"] = "pot-integration-tests",
                ["Jwt:Audience"] = "pot-integration-tests",
                ["Jwt:SecretKey"] = "pot-integration-tests-secret-key-must-be-at-least-64-characters-long-for-hs512",
                ["Smtp:Host"] = "localhost",
                ["Smtp:Port"] = "2525",
                ["Smtp:RequireTls"] = "false",
                ["Smtp:Authentication:Username"] = "test",
                ["Smtp:Authentication:Password"] = "test",
                ["Smtp:From:Name"] = "POT Tests",
                ["Smtp:From:Address"] = "pot-tests@example.com",
                ["Authentication:Cookie:Name"] = "pot-auth",
                ["Authentication:Cookie:Path"] = "/",
                ["Authentication:Cookie:SameSite"] = "Lax",
                ["Authentication:Cookie:SecurePolicy"] = "None",
                ["PlatformAdmin:UserIds"] = string.Empty
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services.AddFakeLogging(options =>
            {
                options.CollectRecordsForDisabledLogLevels = true;
            });
        });
    }
}