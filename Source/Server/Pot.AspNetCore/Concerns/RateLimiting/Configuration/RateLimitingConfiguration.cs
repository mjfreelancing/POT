namespace Pot.AspNetCore.Concerns.RateLimiting.Configuration;

/// <summary>
/// Holds rate limiting configuration for all request types. Bound from the <c>RateLimiting</c>
/// configuration section. Property defaults reflect the production values defined in
/// <see cref="RateLimiterDefaults"/>; the section only needs to be present when
/// overriding those defaults (for example, during E2E test runs).
/// </summary>
public sealed class RateLimitingConfiguration
{
    /// <summary>Rate limit options applied to unauthenticated (anonymous) requests.</summary>
    public AnonymousRateLimiterOptions Anonymous { get; set; } = new();

    /// <summary>Rate limit options applied to authenticated requests.</summary>
    public AuthenticatedRateLimiterOptions Authenticated { get; set; } = new();
}
