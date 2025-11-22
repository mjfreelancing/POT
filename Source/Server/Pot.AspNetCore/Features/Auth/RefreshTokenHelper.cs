using Pot.AspNetCore.Concerns.Auth.Configuration;

namespace Pot.AspNetCore.Features.Auth;

// Helper class for setting and clearing HTTP-only cookies with environment-aware security settings.
internal static class RefreshTokenHelper
{
    // Sets the refresh token as an HTTP-only cookie.
    public static void SetCookie(HttpContext httpContext, string refreshToken, AuthenticationOptions authOptions)
    {
        var cookieConfig = authOptions.Cookie;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,                    // Prevents JavaScript access (XSS protection)
            Secure = cookieConfig.SecureOnly,   // Only sent over HTTPS in production
            SameSite = SameSiteMode.Lax,        // CSRF protection, allows cookie on page refresh
            MaxAge = TimeSpan.FromDays(cookieConfig.MaxAgeDays),
            Domain = cookieConfig.Domain,       // Set for cross-subdomain support (e.g., ".payontime.com.au")
            Path = "/",
            IsEssential = true
        };

        httpContext.Response.Cookies.Append(cookieConfig.Name, refreshToken, cookieOptions);
    }

    // Clears the refresh token cookie (used during logout).
    public static void ClearCookie(HttpContext httpContext, AuthenticationOptions authOptions)
    {
        var cookieConfig = authOptions.Cookie;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = cookieConfig.SecureOnly,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.Zero,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        httpContext.Response.Cookies.Append(cookieConfig.Name, string.Empty, cookieOptions);
    }

    // Gets the refresh token from the HTTP-only cookie.
    public static string? GetFromCookie(HttpContext httpContext, AuthenticationOptions authOptions)
    {
        var cookieConfig = authOptions.Cookie;

        // Returns null for missing entries rather than throwing an Exception
        return httpContext.Request.Cookies[cookieConfig.Name];
    }
}
