using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Integration.Tests.Security;

/// <summary>
/// Proves that a route cannot be anonymous by accident: every mapped endpoint either declares an
/// authorization requirement or is explicitly marked as intentionally anonymous.
/// </summary>
/// <remarks>
/// The host registers no fallback authorization policy, so an endpoint that declares nothing is reachable
/// anonymously. The allow-list is the set of endpoints an author has deliberately marked
/// <c>AllowAnonymous()</c>; everything else must carry its own requirement.
/// </remarks>
public class EndpointAuthorizationMetadataFixture : IntegrationFixtureBase
{
    private const string AuthenticatedUserPolicy = "AuthenticatedUser";

    [Fact]
    public void Should_Require_Authorization_On_Every_Endpoint_Unless_It_Is_Explicitly_Anonymous()
    {
        var unprotected = GetEndpoints()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null)
            .Where(endpoint => !endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Any())
            .Select(DescribeEndpoint)
            .OrderBy(description => description, StringComparer.Ordinal)
            .ToList();

        unprotected.ShouldBeEmpty(
            "every endpoint must declare an authorization requirement, or be explicitly marked AllowAnonymous()");
    }

    [Theory]
    [InlineData("PUT", "/api/users/{id}", AuthenticatedUserPolicy)]
    [InlineData("PUT", "/api/users/{id}/roles", "user:manage")]
    [InlineData("PUT", "/api/users/{id}/status", "user:manage")]
    [InlineData("POST", "/api/users/{id}/resend-invite", "user:manage")]
    [InlineData("PUT", "/api/sites/{id}", "site:manage")]
    public void Should_Declare_The_Expected_Requirement_On_Each_Target_Route(string method, string route, string expectedPolicy)
    {
        var endpoint = GetEndpoints()
            .SingleOrDefault(candidate => Matches(candidate, method, route));

        endpoint.ShouldNotBeNull($"{method} {route} must be mapped");

        var policies = endpoint!.Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Select(data => data.Policy)
            .Where(policy => !string.IsNullOrWhiteSpace(policy))
            .ToList();

        policies.ShouldContain(
            expectedPolicy,
            $"{method} {route} must require '{expectedPolicy}'");
    }

    [Theory]
    [InlineData("GET", "/_health")]
    [InlineData("POST", "/api/auth/login")]
    public async Task Should_Leave_Intentionally_Anonymous_Routes_Reachable_Without_Credentials(string method, string route)
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), route);

        if (HttpMethod.Post.Method.Equals(method, StringComparison.Ordinal))
        {
            request.Content = JsonContent.Create(new { Username = string.Empty, Password = string.Empty });
        }

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldNotBe(
            HttpStatusCode.Unauthorized,
            $"{method} {route} is intentionally anonymous and must stay reachable while signed out");
    }

    private List<Endpoint> GetEndpoints()
    {
        using var scope = CreateScope();

        return scope.ServiceProvider.GetRequiredService<EndpointDataSource>().Endpoints.ToList();
    }

    private static bool Matches(Endpoint endpoint, string method, string route)
    {
        if (endpoint is not RouteEndpoint routeEndpoint)
        {
            return false;
        }

        var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];

        if (!methods.Contains(method, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(
            Normalise(routeEndpoint.RoutePattern.RawText),
            Normalise(route),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string DescribeEndpoint(Endpoint endpoint)
    {
        if (endpoint is not RouteEndpoint routeEndpoint)
        {
            return endpoint.DisplayName ?? endpoint.ToString() ?? "(unknown endpoint)";
        }

        var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];

        return $"{string.Join('/', methods)} /{Normalise(routeEndpoint.RoutePattern.RawText)}";
    }

    private static string Normalise(string? route)
    {
        var trimmed = (route ?? string.Empty).Trim('/');

        // A route constraint - for example {id:guid} - is an implementation detail; the contract names the
        // path shape, so the constraint is dropped from both sides of the comparison.
        return Regex.Replace(trimmed, @"\{([^}:]+):[^}]+\}", "{$1}");
    }
}
