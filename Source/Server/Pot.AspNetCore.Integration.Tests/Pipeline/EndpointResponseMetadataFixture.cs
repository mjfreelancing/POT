using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

public class EndpointResponseMetadataFixture : IntegrationFixtureBase
{
    [Fact]
    public void Should_Declare_Standard_ProblemDetails_For_Every_Api_Endpoint()
    {
        using var scope = CreateScope();

        var apiEndpoints = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>());

        apiEndpoints.ShouldNotBeEmpty();

        var endpointsMissingInternalServerError = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .All(response => response.StatusCode != StatusCodes.Status500InternalServerError
                    || response.Type != typeof(ProblemDetails)
                    || !response.ContentTypes.Contains("application/problem+json", StringComparer.OrdinalIgnoreCase)))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsMissingInternalServerError.ShouldBeEmpty(
            "every API endpoint documents the shared ProblemDetails 500 response");

        var endpointsMissingBadRequest = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .All(response => response.StatusCode != StatusCodes.Status400BadRequest
                    || response.Type != typeof(ProblemDetails)
                    || !response.ContentTypes.Contains("application/problem+json", StringComparer.OrdinalIgnoreCase)))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsMissingBadRequest.ShouldBeEmpty(
            "every API endpoint can return a ProblemDetails 400 for malformed input or an invalid correlation ID");

        var endpointsDocumentingUnprocessableEntity = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .Any(response => response.StatusCode == StatusCodes.Status422UnprocessableEntity))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        // A guard rather than a description: the API answers 400 for every invalid request, so a 422 in the
        // OpenAPI metadata would mean the old contract has crept back in.
        endpointsDocumentingUnprocessableEntity.ShouldBeEmpty("the API should not return 422 responses");
    }

    [Fact]
    public void Should_Declare_Conflict_Responses_As_ProblemDetails()
    {
        using var scope = CreateScope();

        var apiEndpoints = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>());

        var endpointsWithMalformedConflict = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .Any(response => response.StatusCode == StatusCodes.Status409Conflict
                    && (response.Type != typeof(ProblemDetails)
                        || !response.ContentTypes.Contains("application/problem+json", StringComparer.OrdinalIgnoreCase))))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsWithMalformedConflict.ShouldBeEmpty(
            "a declared 409 must describe the shared ProblemDetails conflict response");
    }

    [Fact]
    public void Should_Not_Describe_Success_Responses_As_ProblemDetails()
    {
        using var scope = CreateScope();

        var apiEndpoints = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>());

        var endpointsWithProblemDetailsSuccess = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .Any(response => (response.StatusCode == StatusCodes.Status200OK
                        || response.StatusCode == StatusCodes.Status201Created)
                    && (response.Type == typeof(ProblemDetails)
                        || response.ContentTypes.Contains("application/problem+json", StringComparer.OrdinalIgnoreCase))))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsWithProblemDetailsSuccess.ShouldBeEmpty(
            "successful operations must not advertise a ProblemDetails response body");
    }

    [Fact]
    public void Should_Use_An_Api_Response_Model_For_Accrual_Status()
    {
        using var scope = CreateScope();

        var endpoint = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>())
            .Single(candidate => candidate.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "GetStatus");

        var response = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .First(candidate => candidate.StatusCode == StatusCodes.Status200OK);

        response.Type.ShouldBe(typeof(Pot.AspNetCore.Features.Accruals.Status.Response));
    }

    [Theory]
    [InlineData("GetAllAccounts", StatusCodes.Status200OK, typeof(Pot.AspNetCore.Features.Accounts.GetAll.Response[]), "application/json")]
    [InlineData("CreateAccount", StatusCodes.Status201Created, typeof(Pot.AspNetCore.Features.Accounts.Create.Response), "application/json")]
    [InlineData("LogoutUser", StatusCodes.Status200OK, typeof(void), null)]
    [InlineData("Export", StatusCodes.Status200OK, typeof(void), "application/octet-stream")]
    [InlineData("DeleteAccount", StatusCodes.Status204NoContent, typeof(void), null)]
    [InlineData("DeleteExpense", StatusCodes.Status204NoContent, typeof(void), null)]
    [InlineData("DeleteIncome", StatusCodes.Status204NoContent, typeof(void), null)]
    public void Should_Declare_The_Actual_Success_Response(string endpointName, int statusCode,
        Type? responseType, string? contentType)
    {
        using var scope = CreateScope();

        var endpoint = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>())
            .SingleOrDefault(candidate => candidate.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == endpointName);

        endpoint.ShouldNotBeNull($"{endpointName} must be mapped");

        var responses = endpoint!.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>();
        var declaresExpectedResponse = responses.Any(candidate => candidate.StatusCode == statusCode
            && candidate.Type == responseType
            && (contentType is null
                ? !candidate.ContentTypes.Any()
                : candidate.ContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase)));

        declaresExpectedResponse.ShouldBeTrue($"{endpointName} must document its actual successful response");
    }

    private static List<RouteEndpoint> GetApiEndpoints(EndpointDataSource endpointDataSource)
    {
        return
        [
            .. endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.RoutePattern.RawText?
                    .TrimStart('/')
                    .StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true)
        ];
    }

}
