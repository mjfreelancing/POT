using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Pot.Data.Entities;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

public class LoginFixture : IntegrationFixtureBase
{
    private const string SetCookieHeader = "Set-Cookie";
    private const string RefreshTokenCookieName = "pot_refresh_token";

    [Fact]
    public async Task Should_Create_AuthSession_And_Set_RefreshToken_Cookie_When_Posting_Login_Endpoint()
    {
        var (UserRowId, Username, Password) = await CreateEnabledUserAsync();

        // Disable automatic cookie handling. The refresh token cookie uses HTTP-only and other directives that
        // cause CookieContainerHandler to fail parsing Set-Cookie. This is safe because this test manually extracts
        // the cookie value from the response header (doesn't rely on automatic cookie management for subsequent requests).
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        };

        using var client = CreateClient(clientOptions);

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
        using var scope = CreateScope();

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
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<AuthSessionEntity>()
            .Include(authSession => authSession.User)
            .SingleAsync(authSession => authSession.User.RowId == userRowId);
    }
}