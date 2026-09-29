using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Adds the seeded-user and login helpers that fixtures needing an authenticated caller share.
/// </summary>
/// <remarks>
/// Kept separate from <see cref="IntegrationFixtureBase" />, which stays responsible for the container and host
/// lifecycle alone, so a fixture that never authenticates keeps deriving from it directly.
/// </remarks>
public abstract class IntegrationAuthFixtureBase : IntegrationFixtureBase
{
    /// <summary>
    /// A user seeded for a fixture, with the credentials needed to log in as them.
    /// </summary>
    /// <param name="UserRowId">The user's identifier.</param>
    /// <param name="Username">The username to log in with.</param>
    /// <param name="Password">The password to log in with.</param>
    protected sealed record SeededUser(Guid UserRowId, string Username, string Password);

    /// <summary>
    /// The tokens the login endpoint issues.
    /// </summary>
    /// <param name="AccessToken">The bearer token for subsequent requests.</param>
    /// <param name="RefreshToken">The refresh token the login endpoint sets as a cookie.</param>
    protected sealed record AuthTokens(string AccessToken, string RefreshToken);

    private sealed class LoginResponse
    {
        public string? Status { get; set; }
        public string? AccessToken { get; set; }
    }

    private const string LoginPath = "/api/auth/login";
    private const string LoginSuccessStatus = "Success";

    /// <summary>
    /// The response header the login endpoint sets its refresh token cookie in.
    /// </summary>
    protected const string SetCookieHeader = "Set-Cookie";

    /// <summary>
    /// The password given to every seeded user. The product's password rules are not what these fixtures exercise.
    /// </summary>
    protected const string SeededPassword = "Password123!";

    /// <summary>
    /// The <c>User-Agent</c> a seeded login is recorded against when the fixture does not care about device identity.
    /// </summary>
    protected const string DefaultUserAgent = "POT Integration Test Agent/1.0";

    /// <summary>
    /// The name of the refresh token cookie the login endpoint sets.
    /// </summary>
    protected const string RefreshTokenCookieName = "pot_refresh_token";

    /// <summary>
    /// Seeds a site and an enabled user with no role, for tests that only need a valid identity.
    /// </summary>
    /// <param name="purpose">A short label, unique within its fixture, used to build the username and site name.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <returns>The seeded user and their login credentials.</returns>
    protected Task<SeededUser> CreateEnabledUserAsync(string purpose, string displayName)
    {
        return SeedUserAsync(purpose, displayName, assignAdminRole: false);
    }

    /// <summary>
    /// Seeds a site and an enabled user holding the Admin role, for tests that need permissions.
    /// </summary>
    /// <param name="purpose">A short label, unique within its fixture, used to build the username and site name.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <returns>The seeded user and their login credentials.</returns>
    protected Task<SeededUser> CreateAdminUserAsync(string purpose, string displayName)
    {
        return SeedUserAsync(purpose, displayName, assignAdminRole: true);
    }

    /// <summary>
    /// Logs in as a seeded user and returns the tokens the login endpoint issues.
    /// </summary>
    /// <param name="user">The user to log in as.</param>
    /// <param name="userAgent">The <c>User-Agent</c> the login is recorded against.</param>
    /// <returns>The issued access and refresh tokens.</returns>
    protected async Task<AuthTokens> LoginAsync(SeededUser user, string userAgent = DefaultUserAgent)
    {
        // Cookie handling is off because the refresh token cookie carries directives the automatic
        // CookieContainer handler cannot parse, and these tests read the cookie from the response header instead.
        using var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, LoginPath)
        {
            Content = JsonContent.Create(new { Username = user.Username, Password = user.Password })
        };

        request.Headers.Add("User-Agent", userAgent);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.Status.ShouldBe(LoginSuccessStatus);
        body.AccessToken.ShouldNotBeNullOrWhiteSpace();

        return new AuthTokens(body.AccessToken!, ExtractRefreshToken(response));
    }

    /// <summary>
    /// Logs in as a seeded user and returns a client already carrying the bearer token.
    /// </summary>
    /// <param name="user">The user to log in as.</param>
    /// <param name="userAgent">The <c>User-Agent</c> the login is recorded against.</param>
    /// <returns>A client whose default authorization header is the issued bearer token.</returns>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(SeededUser user, string userAgent = DefaultUserAgent)
    {
        var tokens = await LoginAsync(user, userAgent);
        var client = CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return client;
    }

    /// <summary>
    /// Reads the refresh token the login or refresh endpoint set as a cookie.
    /// </summary>
    /// <param name="response">The response whose <c>Set-Cookie</c> header carries the token.</param>
    /// <returns>The token value alone, without the cookie's other directives.</returns>
    protected static string ExtractRefreshToken(HttpResponseMessage response)
    {
        response.Headers.TryGetValues(SetCookieHeader, out var setCookieValues).ShouldBeTrue();

        var refreshTokenCookie = setCookieValues!
            .FirstOrDefault(cookie => cookie.StartsWith($"{RefreshTokenCookieName}=", StringComparison.Ordinal));

        refreshTokenCookie.ShouldNotBeNull();

        return refreshTokenCookie!
            .Split(';', 2, StringSplitOptions.TrimEntries)[0]
            .Split('=', 2)[1];
    }

    private async Task<SeededUser> SeedUserAsync(string purpose, string displayName, bool assignAdminRole)
    {
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IUserPasswordHasher>();
        var uniqueValue = Guid.NewGuid().ToString("N");
        var site = EntityFactory.CreateSite(name: $"{purpose} Site {uniqueValue}");
        var username = $"{purpose}-{uniqueValue}";

        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", displayName);

        user.PasswordHash = passwordHasher.GetHash(user, SeededPassword);

        dbContext.Add(site);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        if (assignAdminRole)
        {
            // The Admin role and its permission set are seeded by the AddRolesAndPermissions migration, so it is
            // attached rather than added - adding it through the user graph would insert a duplicate role.
            var adminRole = await dbContext.Set<RoleEntity>()
                .AsNoTracking()
                .SingleAsync(role => role.Name == Role.Admin, TestContext.Current.CancellationToken);

            dbContext.Attach(adminRole);
            user.Roles.Add(adminRole);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        return new SeededUser(user.RowId, username, SeededPassword);
    }
}
