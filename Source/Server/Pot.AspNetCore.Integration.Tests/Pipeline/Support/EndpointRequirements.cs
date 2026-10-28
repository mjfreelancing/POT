using AllOverIt.Assertion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// Reads what each mapped endpoint declares about who may call it, which is where an authorization case's
/// expectation comes from.
/// </summary>
public static class EndpointRequirements
{
    /// <summary>
    /// Reads every mapped route's declared requirement, keyed by the operation it answers.
    /// </summary>
    /// <param name="services">The host's services, from a scope.</param>
    /// <returns>The requirement declared by each mapped operation.</returns>
    public static IReadOnlyDictionary<EndpointOperation, EndpointRequirement> Read(IServiceProvider services)
    {
        _ = services.WhenNotNull();

        var requirements = new Dictionary<EndpointOperation, EndpointRequirement>();

        foreach (var endpoint in services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>())
        {
            var requirement = Describe(endpoint);
            var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];

            foreach (var method in methods)
            {
                var operation = EndpointOperation.Create(method, endpoint.RoutePattern.RawText ?? string.Empty);

                requirements.TryAdd(operation, requirement);
            }
        }

        return requirements;
    }

    private static EndpointRequirement Describe(Endpoint endpoint)
    {
        var allowsAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        var policies = endpoint.Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Select(authorizeData => authorizeData.Policy)
            .Where(policy => !string.IsNullOrWhiteSpace(policy))
            .Select(policy => policy!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new EndpointRequirement(allowsAnonymous, policies);
    }
}
