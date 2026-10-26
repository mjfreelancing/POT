using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.ProblemDetails;
using System.Globalization;
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
            int? retryAfterSeconds = null;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                retryAfterSeconds = GetRetryAfterSeconds(retryAfter);

                context.HttpContext.Response.Headers.RetryAfter =
                    retryAfterSeconds.Value.ToString(CultureInfo.InvariantCulture);
            }

            var errorDetail = ApiDetailErrorFactory.CreateTooManyRequests(retryAfterSeconds);

            // Written through IProblemDetailsService so a 429 carries the same content type, correlation id, instance
            // and default detail as every other error response.
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = context.HttpContext,
                ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Detail = errorDetail.ErrorMessage,
                    Status = StatusCodes.Status429TooManyRequests
                }
            };

            var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

            await problemDetailsService.WriteAsync(problemDetailsContext);
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

    // RFC 9110 defines Retry-After as an integer number of seconds, or an HTTP date. TotalSeconds is a
    // double, so it would otherwise emit values such as "59.873" that a strict delta-seconds parser
    // rejects. Rounding up avoids sending the client back before the window reopens, and the minimum of
    // one avoids "0", which reads as "retry immediately".
    internal static int GetRetryAfterSeconds(TimeSpan retryAfter)
    {
        return Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
    }
}
