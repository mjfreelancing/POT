using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using Testcontainers.PostgreSql;

using LogoutHandler = Pot.AspNetCore.Features.Auth.Logout.Handler;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

/// <summary>
/// Integration tests for the logout endpoint.
/// Follows the IAsyncLifetime pattern for test isolation (see LoginFixture for detailed explanation).
/// </summary>
public class LogoutFixture : IAsyncLifetime
{
    private const string SetCookieHeader = "Set-Cookie";
    private const string RefreshTokenCookieName = "pot_refresh_token";
    private const string CorrelationIdProperty = "correlationId";

    private PostgreSqlContainer? _container;
    private ProductionApiWebApplicationFactory? _factory;

    async Task IAsyncLifetime.InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:13")
            .WithDatabase(ApiWebApplicationFactory.TestDatabase)
            .WithUsername(ApiWebApplicationFactory.TestUsername)
            .WithPassword(ApiWebApplicationFactory.TestPassword)
            .Build();

        await _container.StartAsync();

        _factory = new ProductionApiWebApplicationFactory(
            _container.Hostname,
            _container.GetMappedPublicPort(5432));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_factory is not null)
        {
            _factory.Dispose();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    /*
    TODO(logging): Re-enable when the replacement logging test framework is available.
    [Fact]
    public async Task Should_LogCall_When_Posting_Logout_Endpoint()
    {
        var collector = _factory.Services.GetFakeLogCollector();
        collector.Clear();

        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var actual = collector.GetSnapshot()
            .Where(record =>
                record.Category == typeof(LogoutHandler).FullName &&
                record.Level == LogLevel.Information &&
                record.GetStructuredStateValue("MethodName")?.ToString() == nameof(LogoutHandler.Invoke) &&
                record.GetStructuredStateValue("{OriginalFormat}")?.ToString() == "Call: {MethodName}")
            .FirstOrDefault();

        actual.ShouldNotBeNull();

        actual.GetStructuredStateValue(CorrelationIdProperty)
            ?.ToString()
            .ShouldNotBeNullOrWhiteSpace();
    }
    */

    [Fact]
    public async Task Should_Return_MethodNotAllowed_When_Getting_Logout_Endpoint()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        var actual = await client.GetAsync("/api/auth/logout");

        actual.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Should_Return_Ok_When_Posting_Logout_Endpoint_Anonymously()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Clear_RefreshToken_Cookie_When_Posting_Logout_Endpoint()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);
        var setCookieValues = response.ShouldHaveHeaderValues(SetCookieHeader);

        setCookieValues.ShouldContainValue($"{RefreshTokenCookieName}=", StringComparison.Ordinal);
        setCookieValues.ShouldContainValue("Max-Age=0", StringComparison.OrdinalIgnoreCase);
    }
}