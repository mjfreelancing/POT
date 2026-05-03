using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.Data;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testcontainers.PostgreSql;

namespace Pot.AspNetCore.Integration.Tests.Security;

/// <summary>
/// Integration tests for security headers and rate limiting enforcement.
/// Follows the IAsyncLifetime pattern for test isolation (see LoginFixture for detailed explanation).
/// </summary>
public class HeadersAndRateLimitFixture : IAsyncLifetime
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

    private PostgreSqlContainer? _container;
    private ProductionApiWebApplicationFactory? _factory;

    async Task IAsyncLifetime.InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:13")
            .WithDatabase(ApiWebApplicationFactory.TestDatabase)
            .WithUsername(ApiWebApplicationFactory.TestUsername)
            .WithPassword(ApiWebApplicationFactory.TestPassword)
            .Build();

        await _container.StartAsync();

        _factory = new ProductionApiWebApplicationFactory(
            _container.Hostname,
            _container.GetMappedPublicPort(5432));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_factory is not null)
        {
            _factory.Dispose();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    [Fact]
    public async Task Should_Return_TooManyRequests_When_Anonymous_RateLimit_Is_Exceeded()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

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