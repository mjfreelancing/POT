using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

using LogoutHandler = Pot.AspNetCore.Features.Auth.Logout.Handler;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

/// <summary>
/// Integration tests for the logout endpoint.
/// Follows the IAsyncLifetime pattern for test isolation (see LoginFixture for detailed explanation).
/// </summary>
public class LogoutFixture : IAsyncLifetime
{
    private sealed class LoginResponse
    {
        public string? Status { get; set; }
        public string? AccessToken { get; set; }
    }

    private sealed record AuthTokens(string AccessToken, string RefreshToken);

    private const string LoginSuccessStatus = "Success";
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

    [Fact]
    public async Task Should_Not_Revoke_Other_Sessions_When_Logging_Out()
    {
        var (_, username, password) = await CreateEnabledUserAsync();
        var deviceA = await LoginAsync(username, password, "POT Device A/1.0");
        var deviceB = await LoginAsync(username, password, "POT Device B/1.0");

        var logoutStatus = await LogoutAsync(deviceA.AccessToken, deviceA.RefreshToken);

        logoutStatus.ShouldBe(HttpStatusCode.OK);

        var deviceBRefreshStatus = await RefreshAsync(deviceB.AccessToken, deviceB.RefreshToken);

        deviceBRefreshStatus.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Reject_Refresh_After_Logout()
    {
        var (_, username, password) = await CreateEnabledUserAsync();
        var deviceA = await LoginAsync(username, password, "POT Device A/1.0");

        var logoutStatus = await LogoutAsync(deviceA.AccessToken, deviceA.RefreshToken);

        logoutStatus.ShouldBe(HttpStatusCode.OK);

        var refreshStatus = await RefreshAsync(deviceA.AccessToken, deviceA.RefreshToken);

        refreshStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Revoke_All_Sessions_When_Password_Changed()
    {
        const string newPassword = "NewPassword456!";
        var (_, username, password) = await CreateEnabledUserAsync();
        var deviceA = await LoginAsync(username, password, "POT Device A/1.0");
        var deviceB = await LoginAsync(username, password, "POT Device B/1.0");

        var changePasswordStatus = await ChangePasswordAsync(deviceA.AccessToken, password, newPassword);

        changePasswordStatus.ShouldBe(HttpStatusCode.OK);

        var deviceARefreshStatus = await RefreshAsync(deviceA.AccessToken, deviceA.RefreshToken);
        var deviceBRefreshStatus = await RefreshAsync(deviceB.AccessToken, deviceB.RefreshToken);

        deviceARefreshStatus.ShouldBe(HttpStatusCode.Unauthorized);
        deviceBRefreshStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<(Guid UserRowId, string Username, string Password)> CreateEnabledUserAsync()
    {
        _factory.ShouldNotBeNull();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IUserPasswordHasher>();

        var username = $"user_{Guid.NewGuid():N}";
        const string password = "Password123!";

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", "Logout User");

        user.PasswordHash = passwordHasher.GetHash(user, password);

        dbContext.Add(site);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync();

        return (user.RowId, username, password);
    }

    private async Task<AuthTokens> LoginAsync(string username, string password, string userAgent)
    {
        _factory.ShouldNotBeNull();

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = username, Password = password })
        };

        request.Headers.Add("User-Agent", userAgent);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.Status.ShouldBe(LoginSuccessStatus);
        body.AccessToken.ShouldNotBeNullOrWhiteSpace();

        var refreshToken = ExtractRefreshToken(response);

        return new AuthTokens(body.AccessToken!, refreshToken);
    }

    private async Task<HttpStatusCode> LogoutAsync(string accessToken, string refreshToken)
    {
        _factory.ShouldNotBeNull();

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Cookie", $"{RefreshTokenCookieName}={refreshToken}");

        var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    private async Task<HttpStatusCode> RefreshAsync(string accessToken, string refreshToken)
    {
        _factory.ShouldNotBeNull();

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Cookie", $"{RefreshTokenCookieName}={refreshToken}");

        var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    private async Task<HttpStatusCode> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword)
    {
        _factory.ShouldNotBeNull();

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/me/change-password")
        {
            Content = JsonContent.Create(new { CurrentPassword = currentPassword, NewPassword = newPassword })
        };

        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    private static string ExtractRefreshToken(HttpResponseMessage response)
    {
        response.Headers.TryGetValues(SetCookieHeader, out var setCookieValues).ShouldBeTrue();

        var refreshTokenCookie = setCookieValues!
            .FirstOrDefault(cookie => cookie.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal));

        refreshTokenCookie.ShouldNotBeNull();

        return refreshTokenCookie!
            .Split(';', 2, StringSplitOptions.TrimEntries)[0]
            .Split('=', 2)[1];
    }
}