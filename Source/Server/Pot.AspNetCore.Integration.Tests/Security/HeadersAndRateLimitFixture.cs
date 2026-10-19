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
    private const string CorrelationIdExtension = "correlationId";
    private const string ProblemJson = "application/problem+json";
    private const string RetryAfterHeader = "Retry-After";

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
        throttledResponse.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJson);
        problemDetails.Status.ShouldBe((int)HttpStatusCode.TooManyRequests);
        problemDetails.Detail.ShouldNotBeNullOrWhiteSpace();
        GetExtensionString(problemDetails, CorrelationIdExtension).ShouldNotBeNullOrWhiteSpace();
        HasExtension(problemDetails, "errors").ShouldBeTrue();

        var allowOriginValues = throttledResponse.ShouldHaveHeaderValues(AccessControlAllowOrigin);
        allowOriginValues.Single().ShouldBe(AllowedOrigin);
        throttledResponse.ShouldExposeHeader(RetryAfterHeader);

        var retryAfterValue = throttledResponse.ShouldHaveHeaderValues(RetryAfterHeader).Single();
        var retryAfterSeconds = int.Parse(retryAfterValue, NumberStyles.Integer, CultureInfo.InvariantCulture);

        retryAfterSeconds.ShouldBeGreaterThanOrEqualTo(1);
        problemDetails.Detail.ShouldContain($"after {retryAfterSeconds} seconds");
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
        throttledResponse.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJson);
        problemDetails.Status.ShouldBe((int)HttpStatusCode.TooManyRequests);
        problemDetails.Detail.ShouldNotBeNullOrWhiteSpace();
        GetExtensionString(problemDetails, CorrelationIdExtension).ShouldNotBeNullOrWhiteSpace();
        HasExtension(problemDetails, "errors").ShouldBeTrue();

        var allowOriginValues = throttledResponse.ShouldHaveHeaderValues(AccessControlAllowOrigin);
        allowOriginValues.Single().ShouldBe(AllowedOrigin);
        throttledResponse.ShouldExposeHeader(RetryAfterHeader);

        // The authenticated limiter is a sliding window, whose failed lease carries no RetryAfter
        // metadata. The header is therefore omitted and the body must not name a wait.
        throttledResponse.ShouldNotContainHeader(RetryAfterHeader);
        problemDetails.Detail.ShouldBe("Too many requests. Please wait and try again.");
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

    private static string? GetExtensionString(ProblemDetailsResponse problemDetails, string key)
    {
        return problemDetails.Extensions.TryGetValue(key, out var value)
            ? value.GetString()
            : null;
    }
}