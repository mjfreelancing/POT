using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Pot.App.Concerns.Auth;
using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Pot.TestUtils;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.AspNetCore.Integration.Tests.Security;

public class HeadersAndRateLimitFixture : IntegrationFixtureBase
{
    private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

    private sealed class ProblemDetailsResponse
    {
        public string? Detail { get; set; }

        public int? Status { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> Extensions { get; set; } = [];
    }

    private sealed class LoginResponse
    {
        public string? Status { get; set; }
        public string? AccessToken { get; set; }
    }

    private const string AllowedOrigin = "http://localhost:3000";
    private const string LoginSuccessStatus = "Success";

    [Fact]
    public async Task Should_Return_TooManyRequests_When_Anonymous_RateLimit_Is_Exceeded()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);

        for (var index = 0; index < RateLimiterDefaults.AnonymousPermitLimit; index++)
        {
            var response = await client.PostAsync("/api/auth/logout", null);
            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }

        var throttledResponse = await client.PostAsync("/api/auth/logout", null);
        var problemDetails = await ReadProblemDetailsAsync(throttledResponse);

        throttledResponse.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        problemDetails.Status.ShouldBe((int)HttpStatusCode.TooManyRequests);
        HasExtension(problemDetails, "errors").ShouldBeTrue();

        var allowOriginValues = throttledResponse.ShouldHaveHeaderValues(AccessControlAllowOrigin);
        allowOriginValues.Single().ShouldBe(AllowedOrigin);

        if (throttledResponse.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            var retryAfterSeconds = double.Parse(retryAfterValues.Single(), CultureInfo.InvariantCulture);
            retryAfterSeconds.ShouldBeGreaterThan(0d);
        }
    }

    [Fact]
    public async Task Should_Return_TooManyRequests_When_Authenticated_RateLimit_Is_Exceeded()
    {
        var (username, password) = await CreateEnabledUserAsync();
        var accessToken = await LoginAsync(username, password);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        for (var index = 0; index < RateLimiterDefaults.AuthenticatedPermitLimit; index++)
        {
            var response = await client.GetAsync("/api/me");
            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }

        var throttledResponse = await client.GetAsync("/api/me");
        var problemDetails = await ReadProblemDetailsAsync(throttledResponse);

        throttledResponse.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        problemDetails.Status.ShouldBe((int)HttpStatusCode.TooManyRequests);
        HasExtension(problemDetails, "errors").ShouldBeTrue();

        var allowOriginValues = throttledResponse.ShouldHaveHeaderValues(AccessControlAllowOrigin);
        allowOriginValues.Single().ShouldBe(AllowedOrigin);

        if (throttledResponse.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            var retryAfterSeconds = double.Parse(retryAfterValues.Single(), CultureInfo.InvariantCulture);
            retryAfterSeconds.ShouldBeGreaterThan(0d);
        }
    }

    [Fact]
    public async Task Should_Not_Throttle_Authenticated_User_When_Anonymous_Limit_Would_Be_Exceeded()
    {
        var (username, password) = await CreateEnabledUserAsync();
        var accessToken = await LoginAsync(username, password);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        // Send more requests than the anonymous limit allows to confirm authenticated partitioning is applied
        var requestCount = RateLimiterDefaults.AnonymousPermitLimit + 1;

        for (var index = 0; index < requestCount; index++)
        {
            var response = await client.GetAsync("/api/me");
            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }
    }

    private static async Task<ProblemDetailsResponse> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        problemDetails.ShouldNotBeNull();

        return problemDetails;
    }

    private static bool HasExtension(ProblemDetailsResponse problemDetails, string key)
    {
        return problemDetails.Extensions.ContainsKey(key);
    }

    private async Task<(string Username, string Password)> CreateEnabledUserAsync()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IUserPasswordHasher>();

        var username = $"ratelimit_{Guid.NewGuid():N}";
        const string password = "Password123!";

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site, username, $"{username}@example.com", "Rate Limit Test User");

        user.PasswordHash = passwordHasher.GetHash(user, password);

        dbContext.Add(site);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync();

        return (username, password);
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = username, Password = password })
        };

        request.Headers.Add("User-Agent", "POT Rate Limit Test Agent/1.0");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.Status.ShouldBe(LoginSuccessStatus);
        body.AccessToken.ShouldNotBeNullOrWhiteSpace();

        return body.AccessToken!;
    }
}