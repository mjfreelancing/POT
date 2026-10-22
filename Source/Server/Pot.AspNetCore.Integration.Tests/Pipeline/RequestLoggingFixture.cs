using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

// The request log is the framework's HTTP logging: one entry per request carrying the request metadata, never a body or
// a header, and no entry for the health probes, which are short-circuited out of the pipeline. The raw body logger this
// replaces wrote whole payloads, including credentials.
public class RequestLoggingFixture : IntegrationFixtureBase
{
    private const string HttpLoggingCategory = "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware";
    private const string LevelConfigurationKey = "Logging:LogLevel:" + HttpLoggingCategory;
    private const string MePath = "/api/me";
    private const string LoginPath = "/api/auth/login";
    private const string BodySentinel = "request-log-body-sentinel";
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string ContentTypeOptionsHeader = "X-Content-Type-Options";

    private FakeLogCollector GetLogCollector()
    {
        using var scope = CreateScope();

        return scope.ServiceProvider.GetFakeLogCollector();
    }

    private static IReadOnlyList<FakeLogRecord> GetRequestLogEntries(FakeLogCollector collector)
    {
        return collector.GetSnapshot()
            .Where(record => record.Category == HttpLoggingCategory)
            .ToList();
    }

    [Fact]
    public void Should_Configure_The_Request_Log_Category_At_Information()
    {
        using var scope = CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        configuration[LevelConfigurationKey].ShouldBe("Information");
    }

    [Fact]
    public async Task Should_Log_One_Metadata_Entry_For_A_Request()
    {
        var collector = GetLogCollector();
        collector.Clear();

        using var client = CreateClient();
        var response = await client.GetAsync(MePath, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var entry = GetRequestLogEntries(collector).ShouldHaveSingleItem();

        entry.Level.ShouldBe(LogLevel.Information);
        entry.Message.ShouldContain("GET");
        entry.Message.ShouldContain(MePath);
        entry.Message.ShouldContain("401");
    }

    [Fact]
    public async Task Should_Not_Log_A_Request_Body()
    {
        // Development is where the raw body logger ran, so it is the environment that has to prove the body is gone.
        using var factory = new DevelopmentApiWebApplicationFactory(DbHost, DbPort);
        var collector = factory.Services.GetFakeLogCollector();
        collector.Clear();

        using var client = factory.CreateClient();

        var credentials = new
        {
            Username = "unknown-user",
            Password = BodySentinel
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, LoginPath)
        {
            Content = JsonContent.Create(credentials)
        };

        await client.SendAsync(request, TestContext.Current.CancellationToken);

        collector.GetSnapshot().ShouldNotContain(
            record => record.Message.Contains(BodySentinel, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Should_Not_Log_The_Health_Probes()
    {
        var collector = GetLogCollector();
        collector.Clear();

        using var client = CreateClient();
        await client.GetAsync("/_health", TestContext.Current.CancellationToken);
        await client.GetAsync("/_health/ready", TestContext.Current.CancellationToken);

        GetRequestLogEntries(collector).ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Not_Carry_Security_Headers_Or_A_Correlation_Id_On_The_Health_Probes()
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/_health");
        request.Headers.Add(CorrelationIdHeader, "probe-correlation-id");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The health check middleware writes its own Cache-Control and Pragma; those stay. What must not appear is
        // what the application pipeline adds to every other response.
        response.Headers.Contains(ContentTypeOptionsHeader).ShouldBeFalse();
        response.Headers.Contains(CorrelationIdHeader).ShouldBeFalse();
    }
}
