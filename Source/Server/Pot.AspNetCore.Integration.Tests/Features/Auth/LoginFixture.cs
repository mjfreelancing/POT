using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

/// <summary>
/// Integration tests for the login endpoint.
/// 
/// This fixture demonstrates the proper test isolation pattern using TestContainers:
/// 1. IAsyncLifetime.InitializeAsync() creates a fresh postgres:13 container
/// 2. Container connection details are passed to ProductionApiWebApplicationFactory
/// 3. Migrations are applied to the isolated database
/// 4. Each test gets its own dedicated database with a clean schema
/// 5. IAsyncLifetime.DisposeAsync() cleans up the container after all tests complete
/// 
/// See ApiWebApplicationFactory for detailed explanation of the initialization pattern
/// and why this approach is necessary to avoid fallback to localhost.
/// </summary>
public class LoginFixture : IAsyncLifetime
{
    private const string SetCookieHeader = "Set-Cookie";
    private const string RefreshTokenCookieName = "pot_refresh_token";

    private PostgreSqlContainer? _container;
    private ProductionApiWebApplicationFactory? _factory;

    // IAsyncLifetime.InitializeAsync called by xUnit before any test methods run
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

        // Step 3: Create the factory with ACTUAL container connection details
        // This ensures ConfigureWebHost() receives real values, not fallbacks
        _factory = new ProductionApiWebApplicationFactory(
            _container.Hostname,
            _container.GetMappedPublicPort(5432));

        // Step 4: Apply EF Core migrations to the fresh isolated database
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }

    // IAsyncLifetime.DisposeAsync called by xUnit after all test methods complete
    async Task IAsyncLifetime.DisposeAsync()
    {
        // Cleanup in reverse order
        if (_factory is not null)
        {
            _factory.Dispose();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    [Fact]
    public async Task Should_Create_AuthSession_And_Set_RefreshToken_Cookie_When_Posting_Login_Endpoint()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        var (UserRowId, Username, Password) = await CreateEnabledUserAsync();

        // Disable automatic cookie handling. The refresh token cookie uses HTTP-only and other directives that
        // cause CookieContainerHandler to fail parsing Set-Cookie. This is safe because this test manually extracts
        // the cookie value from the response header (doesn't rely on automatic cookie management for subsequent requests).
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        };

        using var client = _factory.CreateClient(clientOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username, Password })
        };

        request.Headers.Add("User-Agent", "POT Integration Test Agent/1.0");

        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var setCookieValues = response.ShouldHaveHeaderValues(SetCookieHeader);

        var refreshTokenCookie = setCookieValues
            .FirstOrDefault(value => value.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal));

        refreshTokenCookie.ShouldNotBeNull();

        var refreshToken = refreshTokenCookie!
            .Split(';', 2, StringSplitOptions.TrimEntries)[0]
            .Split('=', 2)[1];

        refreshToken.ShouldNotBeNullOrWhiteSpace();

        var authSession = await GetAuthSessionAsync(UserRowId);

        authSession.RefreshTokenHash.ShouldNotBe(refreshToken);
        authSession.RefreshTokenHash.ShouldNotBeNullOrWhiteSpace();
        authSession.UserAgent.ShouldBe("POT Integration Test Agent/1.0");
    }

    private async Task<(Guid UserRowId, string Username, string Password)> CreateEnabledUserAsync()
    {
        _factory.ShouldNotBeNull();

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IUserPasswordHasher>();
        var uniqueValue = Guid.NewGuid().ToString("N");
        var site = EntityFactory.CreateSite(name: $"Login Site {uniqueValue}");
        var username = $"login-{uniqueValue}";
        const string password = "Password123!";

        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", "Login User");

        user.PasswordHash = passwordHasher.GetHash(user, password);

        dbContext.Add(site);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync();

        return (user.RowId, username, password);
    }

    private async Task<AuthSessionEntity> GetAuthSessionAsync(Guid userRowId)
    {
        _factory.ShouldNotBeNull();

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<AuthSessionEntity>()
            .Include(authSession => authSession.User)
            .SingleAsync(authSession => authSession.User.RowId == userRowId);
    }
}