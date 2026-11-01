using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Pot.Data.Entities;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

// The unit tests can only show what ForwardedHeadersOptions was set to; forgetting UseForwardedHeaders() in Program leaves
// those options correct and unread. These go through the real pipeline and assert on the three things in the API that read
// the scheme or the remote address: the Location of a 201, the anonymous rate-limit partition and the login session.
//
// Addresses are from the documentation ranges (RFC 5737), so none can be a real caller.
public class ForwardedHeadersFixture : IntegrationAuthFixtureBase
{
    private const string ForwardedForHeader = "X-Forwarded-For";
    private const string ForwardedProtoHeader = "X-Forwarded-Proto";
    private const string AccountsPath = "/api/accounts";
    private const string LoginPath = "/api/auth/login";
    private const string LogoutPath = "/api/auth/logout";

    [Fact]
    public async Task Should_Return_An_Https_Location_When_The_Ingress_Forwards_Https()
    {
        // The ingress terminates TLS and forwards plain HTTP, so without this the URL of every created resource is http://.
        var admin = await CreateAdminUserAsync("forwarded-proto", "Forwarded Proto User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await CreateAccountAsync(client, (ForwardedProtoHeader, "https"));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull().Scheme.ShouldBe("https");
    }

    [Fact]
    public async Task Should_Return_An_Http_Location_When_There_Is_No_Forwarded_Proto()
    {
        // The counterpart to the test above, so it cannot pass because the test host happened to be https already.
        var admin = await CreateAdminUserAsync("forwarded-proto-absent", "Forwarded Proto Absent User");

        using var client = await CreateAuthenticatedClientAsync(admin);

        var response = await CreateAccountAsync(client);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull().Scheme.ShouldBe("http");
    }

    [Fact]
    public async Task Should_Give_Each_Forwarded_Caller_Its_Own_Anonymous_Rate_Limit()
    {
        // Anonymous requests are partitioned by remote address. Read from the connection, that is the ingress for every
        // caller, so one caller using up the limit would throttle every signed-out user at once.
        using var client = CreateClient();

        for (var index = 0; index < RateLimiterDefaults.AnonymousPermitLimit; index++)
        {
            var response = await LogoutAsync(client, "203.0.113.10");

            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }

        var throttledResponse = await LogoutAsync(client, "203.0.113.10");
        var otherCallerResponse = await LogoutAsync(client, "203.0.113.20");

        throttledResponse.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        otherCallerResponse.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Should_Record_The_Forwarded_Address_On_The_Login_Session()
    {
        var user = await CreateEnabledUserAsync("forwarded-for", "Forwarded For User");

        await LoginFromAsync(user, "203.0.113.30");

        var authSession = await GetAuthSessionAsync(user.UserRowId);

        authSession.IpAddress.ShouldBe("203.0.113.30");
    }

    [Fact]
    public async Task Should_Record_Only_The_Last_Hop_So_A_Caller_Cannot_Choose_Its_Own_Address()
    {
        // ForwardLimit is 1: the rightmost entry is the one the ingress wrote. Everything to its left arrived with the
        // request, so a caller could have put it there.
        var user = await CreateEnabledUserAsync("forwarded-for-spoofed", "Forwarded For Spoofed User");

        await LoginFromAsync(user, "198.51.100.1, 203.0.113.40");

        var authSession = await GetAuthSessionAsync(user.UserRowId);

        authSession.IpAddress.ShouldBe("203.0.113.40");
    }

    private static async Task<HttpResponseMessage> CreateAccountAsync(HttpClient client, params (string Name, string Value)[] headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, AccountsPath)
        {
            Content = JsonContent.Create(new
            {
                Description = $"Account {Guid.NewGuid():N}",
                Balance = 1000.0d,
                Reserved = 0.0d
            })
        };

        foreach (var (name, value) in headers)
        {
            request.Headers.Add(name, value);
        }

        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<HttpResponseMessage> LogoutAsync(HttpClient client, string forwardedFor)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, LogoutPath);

        request.Headers.Add(ForwardedForHeader, forwardedFor);

        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task LoginFromAsync(SeededUser user, string forwardedFor)
    {
        // Cookie handling is off because the refresh token cookie carries directives the automatic CookieContainer
        // handler cannot parse, and nothing here needs the cookie.
        using var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, LoginPath)
        {
            Content = JsonContent.Create(new { user.Username, user.Password })
        };

        request.Headers.Add(ForwardedForHeader, forwardedFor);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task<AuthSessionEntity> GetAuthSessionAsync(Guid userRowId)
    {
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();

        return await dbContext.Set<AuthSessionEntity>()
            .Include(authSession => authSession.User)
            .SingleAsync(authSession => authSession.User.RowId == userRowId, TestContext.Current.CancellationToken);
    }
}
