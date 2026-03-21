using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.AspNetCore.Integration.Tests.Security;

public class HeadersAndRateLimitFixture : IClassFixture<ProductionApiWebApplicationFactory>
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

    private readonly ProductionApiWebApplicationFactory _factory;

    public HeadersAndRateLimitFixture(ProductionApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_TooManyRequests_When_Anonymous_RateLimit_Is_Exceeded()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", AllowedOrigin);

        for (var index = 0; index < 15; index++)
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