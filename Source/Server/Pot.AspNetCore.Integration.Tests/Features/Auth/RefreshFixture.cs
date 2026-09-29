using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Pot.Data.Entities;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

public class RefreshFixture : IntegrationAuthFixtureBase
{
    private sealed class RefreshResponse
    {
        public string? AccessToken { get; set; }
    }

    private sealed record AuthResponse(HttpStatusCode StatusCode, string? AccessToken, string? RefreshToken);

    [Fact]
    public async Task Should_Rotate_RefreshToken_And_Update_LastSeenUtc_On_Same_Session_When_Posting_Refresh_Endpoint()
    {
        var user = await CreateEnabledUserAsync("refresh", "Refresh User");
        var loginResult = await LoginAsync(user, "POT Refresh Test Agent/1.0");
        var session = (await GetAuthSessionsAsync(user.UserRowId)).Single();
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

        var refreshedSession = (await GetAuthSessionsAsync(user.UserRowId)).Single();

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
        var user = await CreateEnabledUserAsync("refresh", "Refresh User");
        var deviceA = await LoginAsync(user, "POT Device A/1.0");
        var deviceB = await LoginAsync(user, "POT Device B/1.0");
        var sessionsBeforeRefresh = await GetAuthSessionsAsync(user.UserRowId);
        var deviceASessionBeforeRefresh = sessionsBeforeRefresh.Single(authSession => authSession.UserAgent == "POT Device A/1.0");
        var deviceBSessionBeforeRefresh = sessionsBeforeRefresh.Single(authSession => authSession.UserAgent == "POT Device B/1.0");

        var deviceARefresh = await RefreshAsync(deviceA.AccessToken!, deviceA.RefreshToken!);

        deviceARefresh.StatusCode.ShouldBe(HttpStatusCode.OK);

        var sessionsAfterRefresh = await GetAuthSessionsAsync(user.UserRowId);
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
        var user = await CreateEnabledUserAsync("refresh", "Refresh User");
        var loginResult = await LoginAsync(user, "POT Revoked Session Agent/1.0");
        var session = (await GetAuthSessionsAsync(user.UserRowId)).Single();

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
        var user = await CreateEnabledUserAsync("refresh", "Refresh User");
        var loginResult = await LoginAsync(user, "POT Expired Session Agent/1.0");
        var session = (await GetAuthSessionsAsync(user.UserRowId)).Single();

        await UpdateAuthSessionAsync(session.RowId, authSession =>
        {
            authSession.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);
        });

        var refreshResult = await RefreshAsync(loginResult.AccessToken!, loginResult.RefreshToken!);

        refreshResult.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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
}
