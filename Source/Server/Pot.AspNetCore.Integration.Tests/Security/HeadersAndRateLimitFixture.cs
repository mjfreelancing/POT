using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.AspNetCore.Integration.Tests.Security;

public class HeadersAndRateLimitFixture : IntegrationAuthFixtureBase
{
    private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

    private sealed class ProblemDetailsResponse
    {
        public string? Detail { get; set; }

        public int? Status { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> Extensions { get; set; } = [];
    }

    private const string AllowedOrigin = "http://localhost:3000";

    [Fact]
    public async Task Should_Return_TooManyRequests_When_Anonymous_RateLimit_Is_Exceeded()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);

        for (var index = 0; index < RateLimiterDefaults.AnonymousPermitLimit; index++)
        {
            var response = await client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);
            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }

        var throttledResponse = await client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);
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
        var user = await CreateEnabledUserAsync("ratelimit", "Rate Limit Test User");

        using var client = await CreateAuthenticatedClientAsync(user, "POT Rate Limit Test Agent/1.0");
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);

        for (var index = 0; index < RateLimiterDefaults.AuthenticatedPermitLimit; index++)
        {
            var response = await client.GetAsync("/api/me", TestContext.Current.CancellationToken);
            response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        }

        var throttledResponse = await client.GetAsync("/api/me", TestContext.Current.CancellationToken);
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
        var user = await CreateEnabledUserAsync("ratelimit", "Rate Limit Test User");

        using var client = await CreateAuthenticatedClientAsync(user, "POT Rate Limit Test Agent/1.0");
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);

        // Send more requests than the anonymous limit allows to confirm authenticated partitioning is applied
        var requestCount = RateLimiterDefaults.AnonymousPermitLimit + 1;

        for (var index = 0; index < requestCount; index++)
        {
            var response = await client.GetAsync("/api/me", TestContext.Current.CancellationToken);
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
}