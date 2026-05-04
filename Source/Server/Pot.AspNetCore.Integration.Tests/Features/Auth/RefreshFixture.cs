using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.Data;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

public class RefreshFixture : IntegrationFixtureBase
{
    private sealed class LoginResponse
    {
        public string? Status { get; set; }
        public string? AccessToken { get; set; }
    }

    private sealed class RefreshResponse
    {
        public string? AccessToken { get; set; }
    }

    private sealed record AuthResponse(HttpStatusCode StatusCode, string? AccessToken, string? RefreshToken);

    private const string LoginSuccessStatus = "Success";
    private const string RefreshTokenCookieName = "pot_refresh_token";
    private const string SetCookieHeader = "Set-Cookie";

    [Fact]
    public async Task Should_Rotate_RefreshToken_And_Update_LastSeenUtc_On_Same_Session_When_Posting_Refresh_Endpoint()
    {
        var (userRowId, username, password) = await CreateEnabledUserAsync();
        var loginResult = await LoginAsync(username, password, "POT Refresh Test Agent/1.0");
        var session = (await GetAuthSessionsAsync(userRowId)).Single();
        var originalSessionRowId = session.RowId;
        var originalRefreshTokenHash = session.RefreshTokenHash;
        var previousLastSeenUtc = session.LastSeenUtc ?? session.CreatedUtc;
        var forcedLastSeenUtc = previousLastSeenUtc.AddMinutes(-5);

        await UpdateAuthSessionAsync(originalSessionRowId, authSession =>
        {
            authSession.LastSeenUtc = forcedLastSeenUtc;
        });

        var refreshResult = await RefreshAsync(loginResult.AccessToken!, loginResult.RefreshToken!);

        refreshResult.StatusCode.ShouldBe(HttpStatusCode.OK);
        refreshResult.AccessToken.ShouldNotBeNullOrWhiteSpace();
        refreshResult.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        refreshResult.RefreshToken.ShouldNotBe(loginResult.RefreshToken);

        var refreshedSession = (await GetAuthSessionsAsync(userRowId)).Single();

        refreshedSession.RowId.ShouldBe(originalSessionRowId);
        refreshedSession.RefreshTokenHash.ShouldNotBe(originalRefreshTokenHash);
        refreshedSession.RefreshTokenHash.ShouldNotBe(refreshResult.RefreshToken);
        refreshedSession.LastSeenUtc.ShouldNotBeNull();
        refreshedSession.LastSeenUtc.Value.ShouldBeGreaterThan(forcedLastSeenUtc);

        var staleRefreshAttempt = await RefreshAsync(refreshResult.AccessToken!, loginResult.RefreshToken!);

        staleRefreshAttempt.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Keep_Second_Session_Active_When_First_Session_Is_Refreshed()
    {
        var (userRowId, username, password) = await CreateEnabledUserAsync();
        var deviceA = await LoginAsync(username, password, "POT Device A/1.0");
        var deviceB = await LoginAsync(username, password, "POT Device B/1.0");
        var sessionsBeforeRefresh = await GetAuthSessionsAsync(userRowId);
        var deviceASessionBeforeRefresh = sessionsBeforeRefresh.Single(authSession => authSession.UserAgent == "POT Device A/1.0");
        var deviceBSessionBeforeRefresh = sessionsBeforeRefresh.Single(authSession => authSession.UserAgent == "POT Device B/1.0");

        var deviceARefresh = await RefreshAsync(deviceA.AccessToken!, deviceA.RefreshToken!);

        deviceARefresh.StatusCode.ShouldBe(HttpStatusCode.OK);

        var sessionsAfterRefresh = await GetAuthSessionsAsync(userRowId);
        var deviceASessionAfterRefresh = sessionsAfterRefresh.Single(authSession => authSession.UserAgent == "POT Device A/1.0");
        var deviceBSessionAfterRefresh = sessionsAfterRefresh.Single(authSession => authSession.UserAgent == "POT Device B/1.0");

        sessionsAfterRefresh.Count.ShouldBe(2);
        deviceASessionAfterRefresh.RowId.ShouldBe(deviceASessionBeforeRefresh.RowId);
        deviceASessionAfterRefresh.RefreshTokenHash.ShouldNotBe(deviceASessionBeforeRefresh.RefreshTokenHash);
        deviceBSessionAfterRefresh.RowId.ShouldBe(deviceBSessionBeforeRefresh.RowId);
        deviceBSessionAfterRefresh.RefreshTokenHash.ShouldBe(deviceBSessionBeforeRefresh.RefreshTokenHash);

        var deviceBRefresh = await RefreshAsync(deviceB.AccessToken!, deviceB.RefreshToken!);

        deviceBRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        deviceBRefresh.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Reject_Refresh_When_Session_Has_Been_Revoked()
    {
        var (userRowId, username, password) = await CreateEnabledUserAsync();
        var loginResult = await LoginAsync(username, password, "POT Revoked Session Agent/1.0");
        var session = (await GetAuthSessionsAsync(userRowId)).Single();

        await UpdateAuthSessionAsync(session.RowId, authSession =>
        {
            authSession.RevokedUtc = DateTime.UtcNow;
        });

        var refreshResult = await RefreshAsync(loginResult.AccessToken!, loginResult.RefreshToken!);

        refreshResult.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Reject_Refresh_When_Session_Has_Expired()
    {
        var (userRowId, username, password) = await CreateEnabledUserAsync();
        var loginResult = await LoginAsync(username, password, "POT Expired Session Agent/1.0");
        var session = (await GetAuthSessionsAsync(userRowId)).Single();

        await UpdateAuthSessionAsync(session.RowId, authSession =>
        {
            authSession.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);
        });

        var refreshResult = await RefreshAsync(loginResult.AccessToken!, loginResult.RefreshToken!);

        refreshResult.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<(Guid UserRowId, string Username, string Password)> CreateEnabledUserAsync()
    {
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IUserPasswordHasher>();
        var uniqueValue = Guid.NewGuid().ToString("N");
        var site = EntityFactory.CreateSite(name: $"Refresh Site {uniqueValue}");
        var username = $"refresh-{uniqueValue}";
        const string password = "Password123!";

        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", "Refresh User");

        user.PasswordHash = passwordHasher.GetHash(user, password);

        dbContext.Add(site);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync();

        return (user.RowId, username, password);
    }

    private async Task<AuthResponse> LoginAsync(string username, string password, string userAgent)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions
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

        return new AuthResponse(response.StatusCode, body.AccessToken, refreshToken);
    }

    private async Task<AuthResponse> RefreshAsync(string accessToken, string refreshToken)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Cookie", $"{RefreshTokenCookieName}={refreshToken}");

        var response = await client.SendAsync(request);
        var responseBody = response.StatusCode == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<RefreshResponse>()
            : null;

        var rotatedRefreshToken = response.StatusCode == HttpStatusCode.OK
            ? ExtractRefreshToken(response)
            : null;

        return new AuthResponse(response.StatusCode, responseBody?.AccessToken, rotatedRefreshToken);
    }

    private async Task<List<AuthSessionEntity>> GetAuthSessionsAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<AuthSessionEntity>()
            .Include(authSession => authSession.User)
            .Where(authSession => authSession.User.RowId == userRowId)
            .OrderBy(authSession => authSession.UserAgent)
            .ToListAsync();
    }

    private async Task UpdateAuthSessionAsync(Guid sessionRowId, Action<AuthSessionEntity> update)
    {
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var authSession = await dbContext.Set<AuthSessionEntity>()
            .AsTracking()
            .SingleAsync(session => session.RowId == sessionRowId);

        update(authSession);

        await dbContext.SaveChangesAsync();
    }

    private static string ExtractRefreshToken(HttpResponseMessage response)
    {
        response.Headers.TryGetValues(SetCookieHeader, out var setCookieValues).ShouldBeTrue();

        var refreshTokenCookie = setCookieValues!
            .FirstOrDefault(value => value.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal));

        refreshTokenCookie.ShouldNotBeNull();

        return refreshTokenCookie!
            .Split(';', 2, StringSplitOptions.TrimEntries)[0]
            .Split('=', 2)[1];
    }
}
