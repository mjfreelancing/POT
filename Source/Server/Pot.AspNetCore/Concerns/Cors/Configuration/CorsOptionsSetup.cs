using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Pot.AspNetCore.Concerns.Middleware;

namespace Pot.AspNetCore.Concerns.Cors.Configuration;

// Specifically used to configure CorsOptions after CorsConfiguration has been set up
/// <summary>Applies the configured origins to cross-origin resource sharing options.</summary>
public sealed class CorsOptionsSetup : IConfigureOptions<CorsOptions>
{
    // Without Access-Control-Max-Age, Chromium remembers a preflight for only five seconds. Any request
    // carrying an Authorization header or a JSON body is preflighted, so nearly every call from the
    // front end would otherwise pay a second round trip.
    private static readonly TimeSpan PreflightMaxAge = TimeSpan.FromMinutes(10);

    private readonly CorsConfiguration _corsConfiguration;

    /// <summary>Creates an options configurator for cross-origin resource sharing.</summary>
    /// <param name="corsConfiguration">The configured cross-origin resource sharing settings.</param>
    public CorsOptionsSetup(CorsConfiguration corsConfiguration)
    {
        _corsConfiguration = corsConfiguration;
    }

    /// <summary>Applies configured values to the supplied options instance.</summary>
    /// <param name="options">The options instance to configure or validate.</param>
    public void Configure(CorsOptions options)
    {
        var allowedOrigins = _corsConfiguration.GetAllowedOrigins().ToArray();

        options.AddDefaultPolicy(policy =>
        {
            policy
                // Allow frontend URLs
                .WithOrigins(allowedOrigins)

                .AllowAnyMethod()
                .AllowAnyHeader()

                // Required for the refresh-token cookie. This allows browser-managed credentials such as cookies;
                // bearer-token Authorization headers are allowed by AllowAnyHeader() above. Origins must be
                // explicit: a wildcard origin with AllowCredentials() causes policy construction to fail.
                .AllowCredentials()

                // Browsers hide response headers that are not CORS-safelisted from cross-origin script unless they are exposed:
                //  - 'content-disposition' allows the client to handle the file download correctly (get the filename)
                //  - 'location' is the URL of the resource a 201 Created made. The client takes the new id from the response
                //    body, so nothing reads it today - it is exposed so the 201 contract holds for cross-origin script too
                //  - 'retry-after' allows the client to read how long to wait after a 429 rate-limit rejection
                //  - 'X-Correlation-Id' is the id to quote to support; on a successful response it travels only here
                .WithExposedHeaders(HeaderNames.ContentDisposition, HeaderNames.Location, HeaderNames.RetryAfter, CorrelationIdMiddleware.HeaderName)

                .SetPreflightMaxAge(PreflightMaxAge);
        });
    }
}
