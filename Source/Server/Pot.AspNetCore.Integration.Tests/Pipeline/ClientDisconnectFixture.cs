using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class ClientDisconnectFixture : IntegrationFixtureBase
{
    private sealed class ClientDisconnectApiWebApplicationFactory : ApiWebApplicationFactory
    {
        public ClientDisconnectApiWebApplicationFactory(string dbHost, int dbPort)
            : base(dbHost, dbPort)
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureTestServices(services =>
                services.AddSingleton<IStartupFilter, ClientDisconnectProbe>());
        }
    }

    [Theory]
    [InlineData("operation-canceled")]
    [InlineData("io")]
    public async Task Should_Return_ClientClosedRequest_When_The_Client_Disconnects(string scenario)
    {
        // The custom handler that used to answer this case has been removed. The framework already
        // recognises a client disconnect before calling any IExceptionHandler, for both
        // OperationCanceledException and IOException, so the response must still be 499.
        using var factory = new ClientDisconnectApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"{ClientDisconnectProbe.Path}?scenario={scenario}",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe((HttpStatusCode)499);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_ServerError_When_The_Cancellation_Did_Not_Come_From_The_Client()
    {
        // A server-side cancellation is not a client disconnect, so the framework does not take its 499
        // path. The handlers are called (and return false), and the request ends as a 500 - the same
        // outcome with or without the deleted handler.
        using var factory = new ClientDisconnectApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"{ClientDisconnectProbe.Path}?scenario=server-canceled",
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        body.ShouldNotContain("cancelled by the server");

        using var problemDetails = JsonDocument.Parse(body);
        var problem = problemDetails.RootElement;

        problem.GetProperty("correlationId").GetString().ShouldNotBeNullOrWhiteSpace();

        // A server failure states what happened in 'detail', so the caller has something to show.
        problem.GetProperty("detail").GetString().ShouldBe(ProblemTypes.UnexpectedErrorDetail);
        problem.TryGetProperty("errors", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Return_A_Problem_For_A_Server_Error_When_The_Caller_Does_Not_Accept_Json()
    {
        // A browser navigation sends text/html. The API is JSON-only, so the failure is answered as a problem body
        // rather than a page, and the exception message stays out of it.
        using var factory = new ClientDisconnectApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{ClientDisconnectProbe.Path}?scenario=server-canceled");
        request.Headers.TryAddWithoutValidation("Accept", "text/html").ShouldBeTrue();

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        body.ShouldNotContain("cancelled by the server");

        using var problemDetails = JsonDocument.Parse(body);
        var problem = problemDetails.RootElement;

        problem.GetProperty("detail").GetString().ShouldBe(ProblemTypes.UnexpectedErrorDetail);
        problem.TryGetProperty("errors", out _).ShouldBeFalse();
        problem.GetProperty("correlationId").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Echo_The_CorrelationId_On_A_Server_Error()
    {
        // The exception handler clears the response headers before writing the error, so the echo surviving to a 500
        // depends on CorrelationIdMiddleware writing it as the response starts. A 500 is the response a user is most
        // likely to report.
        using var factory = new ClientDisconnectApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{ClientDisconnectProbe.Path}?scenario=server-canceled");
        request.Headers.Add("X-Correlation-Id", "pot-correlation-id-500");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Headers.GetValues("X-Correlation-Id").ShouldBe(["pot-correlation-id-500"]);
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_For_A_Server_Error_When_The_Caller_Does_Not_Accept_Json()
    {
        // The framework's writer declines a request whose Accept header excludes JSON, and the exception handler then
        // has nothing to write the 500 with.
        using var factory = new ClientDisconnectApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{ClientDisconnectProbe.Path}?scenario=server-canceled");
        request.Headers.TryAddWithoutValidation("Accept", "text/html").ShouldBeTrue();

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        using var problemDetails = JsonDocument.Parse(body);

        problemDetails.RootElement.GetProperty("correlationId").GetString().ShouldNotBeNullOrWhiteSpace();
    }
}
