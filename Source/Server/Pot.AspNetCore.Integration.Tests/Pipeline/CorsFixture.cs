using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Shouldly;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class CorsFixture : IClassFixture<ProductionApiWebApplicationFactory>
{
    private sealed class DelimitedCorsApiWebApplicationFactory : ApiWebApplicationFactory
    {
        private readonly string _delimiter;

        public DelimitedCorsApiWebApplicationFactory(string delimiter)
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

    private readonly ProductionApiWebApplicationFactory _factory;

    public CorsFixture(ProductionApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_Cors_Headers_For_Configured_Origin_Preflight_Request()
    {
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
        using var factory = new DelimitedCorsApiWebApplicationFactory(delimiter);
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