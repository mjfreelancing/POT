using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Concerns.Middleware;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Security;

// Every response carries X-Content-Type-Options: nosniff and Cache-Control: no-store, whichever part of the pipeline
// wrote it. Asserted per route into a response, because an endpoint, the authorization middleware and the exception
// handler each write theirs differently - and the exception handler clears the headers before it writes.
public class SecurityHeadersFixture : IntegrationAuthFixtureBase
{
    private const string ContentTypeOptionsHeader = "X-Content-Type-Options";
    private const string MePath = "/api/me";

    private sealed class FailingApiWebApplicationFactory : ApiWebApplicationFactory
    {
        public FailingApiWebApplicationFactory(string dbHost, int dbPort)
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

    [Fact]
    public async Task Should_Carry_Both_Headers_On_An_Authenticated_Response()
    {
        // A user's own details, which is the case no-store exists for.
        var user = await CreateEnabledUserAsync("headers", "Security Headers Test User");

        using var client = await CreateAuthenticatedClientAsync(user);

        var response = await client.GetAsync(MePath, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
        ShouldCarryBothHeaders(response);
    }

    [Fact]
    public async Task Should_Carry_Both_Headers_When_Authentication_Refuses_The_Request()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(MePath, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        ShouldCarryBothHeaders(response);
    }

    [Fact]
    public async Task Should_Carry_Both_Headers_On_An_Unhandled_Failure()
    {
        // The exception handler clears the response headers before writing the 500.
        using var factory = new FailingApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"{ClientDisconnectProbe.Path}?scenario=server-canceled",
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusAsync(HttpStatusCode.InternalServerError);
        ShouldCarryBothHeaders(response);
    }

    private static void ShouldCarryBothHeaders(HttpResponseMessage response)
    {
        response.ShouldHaveHeaderValues(ContentTypeOptionsHeader).ShouldBe([SecurityHeadersMiddleware.NoSniff]);

        // NoStore rather than an exact value: the exception handler writes "no-cache,no-store" on a 500, which
        // satisfies the rule just as well.
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }
}
