using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pot.Data;
using Shouldly;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// Integration tests for CORS (Cross-Origin Resource Sharing) policy enforcement.
/// Follows the IAsyncLifetime pattern for test isolation (see LoginFixture for detailed explanation).
/// 
/// This fixture demonstrates how custom factories can be created during test execution while
/// still receiving proper container connection details (see DelimitedCorsApiWebApplicationFactory).
/// </summary>
public class CorsFixture : IAsyncLifetime
{
    /// <summary>
    /// Custom factory for testing CORS with different origin delimiters (comma vs semicolon).
    /// Accepts database connection details from the parent fixture's container,
    /// matching the pattern required by ApiWebApplicationFactory.
    /// </summary>
    private sealed class DelimitedCorsApiWebApplicationFactory : ApiWebApplicationFactory
    {
        private readonly string _delimiter;

        /// <summary>
        /// Creates a CORS-test factory with a specific origin delimiter and database connection.
        /// </summary>
        /// <param name="delimiter">Origin delimiter: "," or ";"</param>
        /// <param name="dbHost">Database hostname from parent fixture's container</param>
        /// <param name="dbPort">Database port from parent fixture's container</param>
        public DelimitedCorsApiWebApplicationFactory(string delimiter, string dbHost, int dbPort)
            : base(dbHost, dbPort)
        {
            _delimiter = delimiter;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseEnvironment("Production");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cors:AllowedOrigins"] = $"{AllowedOrigin}{_delimiter}{AllowedOrigin2}"
                });
            });
        }
    }

    private const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
    private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

    private const string AllowedOrigin = "http://localhost:3000";
    private const string AllowedOrigin2 = "https://www.localhost:3000";
    private const string DisallowedOrigin = "https://disallowed.example.com";

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
    public async Task Should_Return_Cors_Headers_For_Configured_Origin_Preflight_Request()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/logout");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);
        var allowOriginValues = response.ShouldHaveHeaderValues(AccessControlAllowOrigin);

        allowOriginValues.Single().ShouldBe(AllowedOrigin);
        response.ShouldContainHeader(AccessControlAllowCredentials);
    }

    [Theory]
    [InlineData(",")]
    [InlineData(";")]
    public async Task Should_Return_Cors_Headers_For_Second_Configured_Origin_Preflight_Request(string delimiter)
    {
        _container.ShouldNotBeNull("Container must be initialized by IAsyncLifetime.InitializeAsync()");

        // Create a custom factory with the same container connection details
        using var factory = new DelimitedCorsApiWebApplicationFactory(
            delimiter,
            _container.Hostname,
            _container.GetMappedPublicPort(5432));

        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/logout");
        request.Headers.Add("Origin", AllowedOrigin2);
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);
        var allowOriginValues = response.ShouldHaveHeaderValues(AccessControlAllowOrigin);

        allowOriginValues.Single().ShouldBe(AllowedOrigin2);
        response.ShouldContainHeader(AccessControlAllowCredentials);
    }

    [Fact]
    public async Task Should_Not_Return_AccessControlAllowOrigin_For_Disallowed_Origin_Preflight_Request()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/logout");
        request.Headers.Add("Origin", DisallowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        response.ShouldNotContainHeader(AccessControlAllowOrigin);
    }

    [Fact]
    public async Task Should_Return_Cors_Headers_For_Configured_Origin_On_Validation_Error_Response()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        request.Headers.Add("Origin", AllowedOrigin);

        var response = await client.SendAsync(request);
        var allowOriginValues = response.ShouldHaveHeaderValues(AccessControlAllowOrigin);

        ((int)response.StatusCode).ShouldBe(422);
        allowOriginValues.Single().ShouldBe(AllowedOrigin);
        response.ShouldContainHeader(AccessControlAllowCredentials);
    }

    [Fact]
    public async Task Should_Not_Return_AccessControlAllowOrigin_For_Disallowed_Origin_On_Validation_Error_Response()
    {
        _factory.ShouldNotBeNull("Factory must be initialized by IAsyncLifetime.InitializeAsync()");

        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        request.Headers.Add("Origin", DisallowedOrigin);

        var response = await client.SendAsync(request);

        ((int)response.StatusCode).ShouldBe(422);
        response.ShouldNotContainHeader(AccessControlAllowOrigin);
    }
}