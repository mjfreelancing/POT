using Microsoft.AspNetCore.Http;
using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class CorrelationAndProblemDetailsFixture : IClassFixture<ProductionApiWebApplicationFactory>
{
    private sealed class ProblemDetailsResponse
    {
        public string? Detail { get; set; }

        public int? Status { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> Extensions { get; set; } = [];
    }

    private const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly ProductionApiWebApplicationFactory _factory;

    public CorrelationAndProblemDetailsFixture(ProductionApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_CorrelationId_Header_Exceeds_Max_Length_On_Anonymous_Endpoint()
    {
        using var client = _factory.CreateClient();

        // Use an anonymous endpoint so the request reaches CorrelationIdMiddleware before any auth challenge short-circuits the pipeline.
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Add(CorrelationIdHeader, new string('a', 129));

        var response = await client.SendAsync(request);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problemDetails.Detail.ShouldBe("CorrelationId exceeds max length of 128 chars");
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        GetExtensionString(problemDetails, "instance").ShouldBe("POST /api/auth/logout");
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Accept_CorrelationId_Header_At_Max_Length()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(CorrelationIdHeader, new string('a', 128));

        var response = await client.SendAsync(request);

        // Should get 401 Unauthorized (not 400 BadRequest), confirming correlation ID was accepted
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Include_Request_CorrelationId_In_ProblemDetails_When_Header_Is_Provided()
    {
        using var client = _factory.CreateClient();

        const string correlationId = "pot-correlation-id-123";

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        request.Headers.Add(CorrelationIdHeader, correlationId);

        var response = await client.SendAsync(request);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        problemDetails.Detail.ShouldBe("One or more validation errors occurred.");
        problemDetails.Status.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        GetExtensionString(problemDetails, "correlationId").ShouldBe(correlationId);
        GetExtensionString(problemDetails, "instance").ShouldBe("POST /api/auth/login");
        HasExtension(problemDetails, "errors").ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Include_Generated_CorrelationId_In_ProblemDetails_When_Header_Is_Not_Provided()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty })
        };

        var response = await client.SendAsync(request);
        var problemDetails = await ReadProblemDetailsAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        GetExtensionString(problemDetails, "correlationId").ShouldNotBeNullOrWhiteSpace();
        GetExtensionString(problemDetails, "instance").ShouldBe("POST /api/auth/login");
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
        if (!problemDetails.Extensions.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ToString();
    }
}