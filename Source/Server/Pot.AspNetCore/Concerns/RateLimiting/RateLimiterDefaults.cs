namespace Pot.AspNetCore.Concerns.RateLimiting;

/// <summary>
/// Production default values for the rate limiter. These are the source of truth for all
/// rate limiting behaviour and are applied via <see cref="Configuration.AnonymousRateLimiterOptions"/>
/// and <see cref="Configuration.AuthenticatedRateLimiterOptions"/> property initialisers.
/// Overriding these defaults at runtime (for example in E2E test runs) is done exclusively
/// through configuration — never by changing these constants.
/// </summary>
internal static class RateLimiterDefaults
{
    /// <summary>
    /// Maximum requests an anonymous client may make within <see cref="AnonymousWindowSeconds"/>.
    /// Sized to accommodate a typical login flow (page load + login attempt + OTP) while
    /// blocking rapid brute-force or scraping from a single IP.
    /// </summary>
    internal const int AnonymousPermitLimit = 15;

    /// <summary>
    /// Duration of the anonymous fixed-window in seconds. Short enough to block abuse,
    /// long enough for legitimate anonymous flows, and resets before the frontend request
    /// timeout elapses.
    /// </summary>
    internal const int AnonymousWindowSeconds = 10;

    /// <summary>
    /// Maximum requests an authenticated user may make within <see cref="AuthenticatedWindowSeconds"/>.
    /// Generous enough for normal SPA usage patterns (page load bursts of 5–10 requests,
    /// then occasional interactions).
    /// </summary>
    internal const int AuthenticatedPermitLimit = 50;

    /// <summary>
    /// Duration of the authenticated sliding-window in seconds. Long enough to absorb
    /// page-load bursts; short enough that a user who hits the limit does not wait long
    /// before the window begins to slide open again.
    /// </summary>
    internal const int AuthenticatedWindowSeconds = 30;

    /// <summary>
    /// Number of segments the authenticated sliding window is divided into. More segments
    /// means finer-grained expiry of older request counts, producing smoother rate limiting.
    /// </summary>
    internal const int AuthenticatedSegmentsPerWindow = 10;

    /// <summary>
    /// Queue limit applied to all rate limiters. Zero means requests are rejected immediately
    /// when the permit limit is reached rather than being held in a queue, providing fast
    /// feedback to the client.
    /// </summary>
    internal const int NoQueueLimit = 0;
}
