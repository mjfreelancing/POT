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
                    || response.Type != typeof(HttpValidationProblemDetails)
                    || !response.ContentTypes.Contains("application/problem+json", StringComparer.OrdinalIgnoreCase)))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsMissingBadRequest.ShouldBeEmpty(
            "every API endpoint can return the validation problem for malformed input or an invalid correlation ID");

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

    [Fact]
    public void Should_Declare_Every_Bad_Request_As_The_Validation_Problem()
    {
        using var scope = CreateScope();

        var apiEndpoints = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>());

        var endpointsWithoutTheValidationProblem = apiEndpoints
            .Where(endpoint => !DeclaresOnlyTheValidationProblem(endpoint))
            .Select(endpoint => $"{endpoint.RoutePattern.RawText} -> {DescribeBadRequest(endpoint)}")
            .ToList();

        endpointsWithoutTheValidationProblem.ShouldBeEmpty(
            "every API endpoint answers a 400 with the validation problem, and with nothing else");
    }

    [Fact]
    public void Should_Declare_Payload_Too_Large_And_Request_Timeout_For_Body_Accepting_Endpoints()
    {
        using var scope = CreateScope();

        var apiEndpoints = GetApiEndpoints(scope.ServiceProvider.GetRequiredService<EndpointDataSource>());

        var endpointsAcceptingABody = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAcceptsMetadata>() is not null)
            .ToList();

        endpointsAcceptingABody.ShouldNotBeEmpty();

        var endpointsMissingBodyProblems = endpointsAcceptingABody
            .Where(endpoint => !DeclaresStatus(endpoint, StatusCodes.Status413PayloadTooLarge)
                || !DeclaresStatus(endpoint, StatusCodes.Status408RequestTimeout))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsMissingBodyProblems.ShouldBeEmpty(
            "an endpoint that reads a request body can be refused for its size and for taking too long");

        var endpointsWithoutABodyDeclaringBodyProblems = apiEndpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAcceptsMetadata>() is null)
            .Where(endpoint => DeclaresStatus(endpoint, StatusCodes.Status413PayloadTooLarge)
                || DeclaresStatus(endpoint, StatusCodes.Status408RequestTimeout))
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToList();

        endpointsWithoutABodyDeclaringBodyProblems.ShouldBeEmpty(
            "an endpoint that reads no request body can answer neither 413 nor 408");
    }

    private static bool DeclaresOnlyTheValidationProblem(RouteEndpoint endpoint)
    {
        var badRequests = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Where(response => response.StatusCode == StatusCodes.Status400BadRequest)
            .ToList();

        return badRequests.Count == 1
            && badRequests[0].Type == typeof(HttpValidationProblemDetails)
            && badRequests[0].ContentTypes.Contains("application/problem+json", StringComparer.OrdinalIgnoreCase);
    }

    private static string DescribeBadRequest(RouteEndpoint endpoint)
    {
        var badRequests = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Where(response => response.StatusCode == StatusCodes.Status400BadRequest)
            .Select(response => $"{response.Type?.Name ?? "no type"} ({string.Join(", ", response.ContentTypes)})")
            .ToList();

        return badRequests.Count == 0
            ? "no 400 declared"
            : string.Join(" | ", badRequests);
    }

    private static bool DeclaresStatus(RouteEndpoint endpoint, int statusCode)
    {
        return endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Any(response => response.StatusCode == statusCode);
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
