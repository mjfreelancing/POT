using Microsoft.AspNetCore.Http;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class CorrelationAndProblemDetailsFixture : IntegrationFixtureBase
{
    private sealed class ProblemDetailsResponse
    {
        public string? Detail { get; set; }

        public string? Instance { get; set; }

        public int? Status { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> Extensions { get; set; } = [];
    }

    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
    private const string AllowedOrigin = "http://localhost:3000";
    private const string InvalidCharactersDetail = "CorrelationId may only contain letters, digits, '-', '_', '.' and ':'";

    private static readonly JsonSerializerOptions WebJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Should_Return_BadRequest_Before_Authentication_When_CorrelationId_Header_Exceeds_Max_Length()
    {
        using var client = CreateClient();

        // A protected endpoint and no credentials: a 400 rather than a 401 shows the correlation id is checked first.
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(CorrelationIdHeader, new string('a', 129));

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problemDetails.Detail.ShouldBe("CorrelationId exceeds max length of 128 chars");
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Instance.ShouldBe("/api/me");
        HasExtension(problemDetails, "instance").ShouldBeFalse();
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("pot correlation")]
    [InlineData("pot\"correlation")]
    [InlineData("<script>")]
    [InlineData("pot/correlation")]
    public async Task Should_Return_BadRequest_Before_Authentication_When_CorrelationId_Header_Has_Invalid_Characters(string correlationId)
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.TryAddWithoutValidation(CorrelationIdHeader, correlationId).ShouldBeTrue();

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problemDetails.Detail.ShouldBe(InvalidCharactersDetail);
        GetExtensionString(problemDetails, "correlationId").ShouldNotBe(correlationId);
    }

    [Fact]
    public async Task Should_Accept_CorrelationId_Header_With_All_Allowed_Characters()
    {
        using var client = CreateClient();

        const string correlationId = "Pot-0123456789_abc.XYZ:9";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(CorrelationIdHeader, correlationId);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        GetExtensionString(problemDetails, "correlationId").ShouldBe(correlationId);
    }

    [Fact]
    public async Task Should_Return_Cors_Headers_When_CorrelationId_Header_Is_Rejected()
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.TryAddWithoutValidation(CorrelationIdHeader, "pot correlation").ShouldBeTrue();

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldHaveHeaderValues(AccessControlAllowOrigin).Single().ShouldBe(AllowedOrigin);
    }

    [Fact]
    public async Task Should_Include_Request_CorrelationId_In_ProblemDetails_When_Request_Is_Unauthorized()
    {
        using var client = CreateClient();

        const string correlationId = "pot-correlation-id-401";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(CorrelationIdHeader, correlationId);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        GetExtensionString(problemDetails, "correlationId").ShouldBe(correlationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_Use_Generated_CorrelationId_When_Header_Is_Empty_Or_Whitespace(string correlationId)
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        request.Headers.TryAddWithoutValidation(CorrelationIdHeader, correlationId).ShouldBeTrue();

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var problemDetails = await ReadProblemDetailsAsync(response);

        // The validation error, not a correlation id rejection, shows the header was treated as absent.
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problemDetails.Detail.ShouldBe("One or more validation errors occurred.");
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Accept_CorrelationId_Header_At_Max_Length()
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(CorrelationIdHeader, new string('a', 128));

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Should get 401 Unauthorized (not 400 BadRequest), confirming correlation ID was accepted
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Include_Request_CorrelationId_In_ProblemDetails_When_Header_Is_Provided()
    {
        using var client = CreateClient();

        const string correlationId = "pot-correlation-id-123";

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        request.Headers.Add(CorrelationIdHeader, correlationId);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await ReadProblemDetailsAsync(response);

        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Detail.ShouldBe("One or more validation errors occurred.");
        GetExtensionString(problemDetails, "correlationId").ShouldBe(correlationId);
        problemDetails.Instance.ShouldBe("/api/auth/login");
        HasExtension(problemDetails, "instance").ShouldBeFalse();
        HasExtension(problemDetails, "errors").ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Include_Generated_CorrelationId_In_ProblemDetails_When_Header_Is_Not_Provided()
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await ReadProblemDetailsAsync(response);

        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
        problemDetails.Instance.ShouldBe("/api/auth/login");
        HasExtension(problemDetails, "instance").ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_When_Request_Json_Is_Malformed()
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = new StringContent("{", Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await ReadProblemDetailsAsync(response);

        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_When_Request_Json_Is_Malformed_In_Development()
    {
        // Development turns on ThrowOnBadRequest, so the framework throws BadHttpRequestException instead
        // of returning a 400 itself. BadRequestExceptionHandler must still answer 400, not 500.
        using var factory = new DevelopmentApiWebApplicationFactory(DbHost, DbPort);
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = new StringContent("{", Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await ReadProblemDetailsAsync(response);

        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
    }

    private static async Task<ProblemDetailsResponse> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        content.ShouldNotBeNullOrWhiteSpace("the response should contain a ProblemDetails body");

        var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(
            content,
            WebJsonSerializerOptions);

        problemDetails.ShouldNotBeNull();

        return problemDetails;
    }

    private static bool HasExtension(ProblemDetailsResponse problemDetails, string key)
    {
        return problemDetails.Extensions.ContainsKey(key);
    }

    private static string? GetExtensionString(ProblemDetailsResponse problemDetails, string key)
    {
        if (!problemDetails.Extensions.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ToString();
    }
}