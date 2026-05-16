namespace Pot.AspNetCore.Concerns.RateLimiting.Configuration;

/// <summary>
/// Rate limit options for anonymous (unauthenticated) requests. Requests are partitioned by
/// remote IP address and subject to a fixed-window limiter.
/// </summary>
public sealed class AnonymousRateLimiterOptions
{
    /// <summary>Maximum number of requests permitted within the window.</summary>
    public int PermitLimit { get; set; } = RateLimiterDefaults.AnonymousPermitLimit;

    /// <summary>Duration of the rate limit window, in seconds.</summary>
    public int WindowSeconds { get; set; } = RateLimiterDefaults.AnonymousWindowSeconds;
}
