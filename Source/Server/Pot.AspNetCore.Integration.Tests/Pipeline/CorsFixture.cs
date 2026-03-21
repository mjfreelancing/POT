using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class CorsFixture : IClassFixture<ProductionApiWebApplicationFactory>
{
    private const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
    private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

    private const string AllowedOrigin = "http://localhost:3000";
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