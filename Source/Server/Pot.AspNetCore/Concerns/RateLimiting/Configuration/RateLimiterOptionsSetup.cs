using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Pot.App.Errors;
using Pot.AspNetCore.Extensions;
using System.Threading.RateLimiting;

namespace Pot.AspNetCore.Concerns.RateLimiting.Configuration;

/// <summary>
/// Configures <see cref="RateLimiterOptions"/> using the resolved <see cref="RateLimitingConfiguration"/>.
/// Sets the 429 rejection status code, writes a ProblemDetails response with a <c>Retry-After</c>
/// header when available, and registers the chained rate limit policy via
/// <see cref="RateLimiterPolicy.CreateChainedPolicy"/>.
/// </summary>
internal sealed class RateLimiterOptionsSetup : IConfigureOptions<RateLimiterOptions>
{
    private readonly RateLimitingConfiguration _rateLimitingConfiguration;

    public RateLimiterOptionsSetup(RateLimitingConfiguration rateLimitingConfiguration)
    {
        _rateLimitingConfiguration = rateLimitingConfiguration;
    }

    public void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";
            }

            var errorDetail = ApiDetailErrorFactory.CreateTooManyRequests(retryAfter.TotalSeconds);
            await context.HttpContext.Response.WriteAsJsonAsync(errorDetail.ToProblemDetails(), cancellationToken);
        };

        var anonymousWindow = TimeSpan.FromSeconds(_rateLimitingConfiguration.Anonymous.WindowSeconds);
        var authenticatedWindow = TimeSpan.FromSeconds(_rateLimitingConfiguration.Authenticated.WindowSeconds);

        options.AddPolicy(RateLimiterPolicy.Chained,
            context => RateLimiterPolicy.CreateChainedPolicy(
                context,
                _rateLimitingConfiguration.Anonymous.PermitLimit,
                anonymousWindow,
                _rateLimitingConfiguration.Authenticated.PermitLimit,
                authenticatedWindow));
    }
}
