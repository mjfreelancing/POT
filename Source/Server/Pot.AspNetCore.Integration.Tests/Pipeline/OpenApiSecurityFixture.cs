using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class OpenApiSecurityFixture : IntegrationFixtureBase
{
    [Fact]
    public async Task Should_Describe_BadRequest_ProblemDetails_For_Api_Operations()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var loginOperation = openApiDocument!.RootElement.GetProperty("paths")
            .GetProperty("/api/auth/login")
            .GetProperty("post");
        var badRequestResponse = loginOperation.GetProperty("responses").GetProperty("400");
        var problemDetailsContent = badRequestResponse.GetProperty("content")
            .GetProperty("application/problem+json");

        problemDetailsContent.GetProperty("schema")
            .GetProperty("$ref")
            .GetString()
            .ShouldBe("#/components/schemas/ProblemDetails");
    }

    [Fact]
    public async Task Should_Declare_Bearer_Security_For_Protected_Api_Operations_Only()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseContent);

        using var openApiDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(
            TestContext.Current.CancellationToken);

        openApiDocument.ShouldNotBeNull();

        var root = openApiDocument!.RootElement;
        var bearerScheme = root.GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");

        bearerScheme.GetProperty("type").GetString().ShouldBe("http");
        bearerScheme.GetProperty("scheme").GetString().ShouldBe("bearer");
        bearerScheme.GetProperty("bearerFormat").GetString().ShouldBe("JWT");

        var apiOperations = root.GetProperty("paths")
            .EnumerateObject()
            .Where(path => path.Name.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => path.Value.EnumerateObject()
                .Where(operation => IsHttpMethod(operation.Name))
                .Select(operation => (Path: path.Name, Operation: operation.Value)))
            .ToList();

        var protectedOperations = apiOperations
            .Where(apiOperation => !apiOperation.Path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var anonymousAuthOperations = apiOperations
            .Where(apiOperation => apiOperation.Path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        protectedOperations.ShouldNotBeEmpty();
        anonymousAuthOperations.ShouldNotBeEmpty();

        foreach (var apiOperation in protectedOperations)
        {
            var security = apiOperation.Operation.GetProperty("security");
            security.GetArrayLength().ShouldBe(1);
            security[0].GetProperty("Bearer").GetArrayLength().ShouldBe(0);
        }

        foreach (var apiOperation in anonymousAuthOperations)
        {
            apiOperation.Operation.TryGetProperty("security", out _)
                .ShouldBeFalse($"{apiOperation.Path} should remain anonymous in OpenAPI");
        }
    }

    protected override ApiWebApplicationFactory CreateApiWebApplicationFactory(string dbHost, int dbPort)
    {
        return new DevelopmentApiWebApplicationFactory(dbHost, dbPort);
    }

    private static bool IsHttpMethod(string operationName)
    {
        return operationName is "get" or "post" or "put" or "delete" or "patch" or "options" or "head" or "trace";
    }
}
