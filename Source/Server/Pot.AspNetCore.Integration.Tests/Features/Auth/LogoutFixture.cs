using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

public class LogoutFixture : IntegrationAuthFixtureBase
{
    private const string CorrelationIdProperty = "correlationId";

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
        using var client = CreateClient();

        var actual = await client.GetAsync("/api/auth/logout", TestContext.Current.CancellationToken);

        actual.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Should_Return_Ok_When_Posting_Logout_Endpoint_Anonymously()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Clear_RefreshToken_Cookie_When_Posting_Logout_Endpoint()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);
        var setCookieValues = response.ShouldHaveHeaderValues(SetCookieHeader);

        setCookieValues.ShouldContainValue($"{RefreshTokenCookieName}=", StringComparison.Ordinal);
        setCookieValues.ShouldContainValue("Max-Age=0", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Not_Revoke_Other_Sessions_When_Logging_Out()
    {
        var user = await CreateEnabledUserAsync("logout", "Logout User");
        var deviceA = await LoginAsync(user, "POT Device A/1.0");
        var deviceB = await LoginAsync(user, "POT Device B/1.0");

        var logoutStatus = await LogoutAsync(deviceA.AccessToken, deviceA.RefreshToken);

        logoutStatus.ShouldBe(HttpStatusCode.OK);

        var deviceBRefreshStatus = await RefreshAsync(deviceB.AccessToken, deviceB.RefreshToken);

        deviceBRefreshStatus.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Reject_Refresh_After_Logout()
    {
        var user = await CreateEnabledUserAsync("logout", "Logout User");
        var deviceA = await LoginAsync(user, "POT Device A/1.0");

        var logoutStatus = await LogoutAsync(deviceA.AccessToken, deviceA.RefreshToken);

        logoutStatus.ShouldBe(HttpStatusCode.OK);

        var refreshStatus = await RefreshAsync(deviceA.AccessToken, deviceA.RefreshToken);

        refreshStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Revoke_All_Sessions_When_Password_Changed()
    {
        const string newPassword = "NewPassword456!";
        var user = await CreateEnabledUserAsync("logout", "Logout User");
        var deviceA = await LoginAsync(user, "POT Device A/1.0");
        var deviceB = await LoginAsync(user, "POT Device B/1.0");

        var changePasswordStatus = await ChangePasswordAsync(deviceA.AccessToken, user.Password, newPassword);

        changePasswordStatus.ShouldBe(HttpStatusCode.OK);

        var deviceARefreshStatus = await RefreshAsync(deviceA.AccessToken, deviceA.RefreshToken);
        var deviceBRefreshStatus = await RefreshAsync(deviceB.AccessToken, deviceB.RefreshToken);

        deviceARefreshStatus.ShouldBe(HttpStatusCode.Unauthorized);
        deviceBRefreshStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Not_Increment_TokenVersion_When_Logging_Out()
    {
        var user = await CreateEnabledUserAsync("logout", "Logout User");
        var deviceA = await LoginAsync(user, "POT Device A/1.0");

        var tokenVersionBeforeLogout = await GetUserTokenVersionAsync(user.UserRowId);

        var logoutStatus = await LogoutAsync(deviceA.AccessToken, deviceA.RefreshToken);

        logoutStatus.ShouldBe(HttpStatusCode.OK);

        var tokenVersionAfterLogout = await GetUserTokenVersionAsync(user.UserRowId);

        tokenVersionAfterLogout.ShouldBe(tokenVersionBeforeLogout);
    }

    [Fact]
    public async Task Should_Increment_TokenVersion_When_Password_Is_Changed()
    {
        const string newPassword = "NewPassword789!";
        var user = await CreateEnabledUserAsync("logout", "Logout User");
        var deviceA = await LoginAsync(user, "POT Device A/1.0");

        var tokenVersionBeforeChange = await GetUserTokenVersionAsync(user.UserRowId);

        var changePasswordStatus = await ChangePasswordAsync(deviceA.AccessToken, user.Password, newPassword);

        changePasswordStatus.ShouldBe(HttpStatusCode.OK);

        var tokenVersionAfterChange = await GetUserTokenVersionAsync(user.UserRowId);

        tokenVersionAfterChange.ShouldBe(tokenVersionBeforeChange + 1);
    }

    private async Task<HttpStatusCode> LogoutAsync(string accessToken, string refreshToken)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions
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
        using var client = CreateClient(new WebApplicationFactoryClientOptions
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
        using var client = CreateClient(new WebApplicationFactoryClientOptions
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

    private async Task<int> GetUserTokenVersionAsync(Guid userRowId)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        var tokenVersion = await dbContext.Set<UserEntity>()
            .Where(user => user.RowId == userRowId)
            .Select(user => user.TokenVersion)
            .SingleAsync();

        return tokenVersion;
    }
}