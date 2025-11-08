using AllOverIt.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Pot.AspNetCore.Concerns.RateLimiting;

internal static class RateLimiterPolicy
{
    // Single chained policy applied to all endpoints
    // Combines concurrency limiting with per-user/per-IP rate limiting
    public const string Chained = "ChainedPolicy";

    // Single chained policy applied to all endpoints
    // Strategy: Fail-fast with short windows to align with frontend timeout expectations (10 seconds)
    // 
    // Design Principles:
    // - Short windows (10-30 seconds) prevent long waits that exceed frontend timeouts
    // - No queuing - fail immediately when limits exceeded for clear user feedback
    // - Authenticated users get generous limits (they're trusted, paying customers)
    // - Anonymous users get strict limits (potential attackers, scrapers, bots)
    // - Limits designed for typical SPA usage patterns (burst on page load, then occasional requests)
    public static RateLimitPartition<string> CreateChainedPolicy(HttpContext httpContext)
    {
        var subject = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAuthenticated = subject.IsNotNullOrEmpty();

        // Partition key for rate limiting: use userId for authenticated, IP for anonymous
        var partitionKey = isAuthenticated
            ? $"user:{subject}"
            : $"ip:{httpContext.Connection.RemoteIpAddress}";

        if (isAuthenticated)
        {
            // AUTHENTICATED USERS: Sliding window for smooth, fair rate limiting
            // Sliding window is ideal for known users - smoother than fixed window, no burst gaming
            // 
            // Configuration rationale:
            // - 50 requests per 30 seconds: Generous for legitimate use
            //   * Page load: 5-10 requests
            //   * Typical interaction: 1-3 requests
            //   * Allows ~1.67 requests/second sustained
            // - For 5 users: max 250 requests/30sec if all active simultaneously
            // - 10 segments: Smooth rate limiting, requests expire gradually
            // - NO queue: Fail fast if limit exceeded (better UX than waiting)
            // 
            // Why 30 seconds?
            // - Long enough to handle page load bursts
            // - Short enough that users don't wait long if they hit the limit
            // - Aligns with typical user interaction patterns
            return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 50,                       // 50 requests per window
                Window = TimeSpan.FromSeconds(30),      // 30 second window
                SegmentsPerWindow = 10,                 // 10 segments = 3 sec each
                QueueLimit = 0,                         // No queueing - fail fast
            });
        }
        else
        {
            // ANONYMOUS USERS: Fixed window with very strict limits
            // Fixed window is simpler and more aggressive for untrusted traffic
            // 
            // Configuration rationale:
            // - 15 requests per 10 seconds: Enough for login flow
            //   * Initial page load: 2-3 requests
            //   * Login attempt: 1 request
            //   * Signup flow: 2-4 requests (form + OTP)
            //   * Allows 1.5 requests/second sustained
            // - Prevents brute force: 15 password attempts = 10 second lockout
            // - Prevents DDoS from single IP
            // - Prevents API scraping/enumeration
            // - NO queue: Fail fast to conserve resources and provide clear feedback
            // 
            // Why 10 seconds?
            // - Short enough to prevent abuse
            // - Long enough for legitimate anonymous actions (login, signup)
            // - Window resets before frontend timeout (10s) expires
            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,                       // 15 requests per window
                Window = TimeSpan.FromSeconds(10),      // 10 second window
                QueueLimit = 0,                         // No queueing - fail fast
            });
        }
    }
}
