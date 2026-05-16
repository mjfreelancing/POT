namespace Pot.AspNetCore.Concerns.RateLimiting.Configuration;

/// <summary>
/// Rate limit options for authenticated requests. Requests are partitioned by user identifier
/// and subject to a sliding-window limiter.
/// </summary>
public sealed class AuthenticatedRateLimiterOptions
{
    /// <summary>Maximum number of requests permitted within the window.</summary>
    public int PermitLimit { get; set; } = RateLimiterDefaults.AuthenticatedPermitLimit;

    /// <summary>Duration of the rate limit window, in seconds.</summary>
    public int WindowSeconds { get; set; } = RateLimiterDefaults.AuthenticatedWindowSeconds;
}
