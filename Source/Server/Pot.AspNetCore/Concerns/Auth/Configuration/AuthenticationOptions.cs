namespace Pot.AspNetCore.Concerns.Auth.Configuration;

// Configuration options for authentication features including cookie settings.
// Bound from appsettings.json Authentication section.
public sealed class AuthenticationOptions
{
    private const string RefreshTokenName = "pot_refresh_token";

    public sealed class CookieOptions
    {
        // The name of the refresh token cookie.
        public string Name => RefreshTokenName;

        // When true, cookies are only sent over HTTPS connections (required for production).
        // Set to false for local HTTP development.
        public bool SecureOnly { get; set; } = true;

        // Cookie lifetime in days.
        public int MaxAgeDays { get; set; } = 30;

        // The domain scope for the cookie. Use ".domain.com" for cross-subdomain cookies.
        // Leave null for same-origin cookies (local development).
        public string? Domain { get; set; }
    }

    // The refresh token cookie-specific authentication settings.
    public CookieOptions Cookie { get; set; } = null!;      // Cannot use required - doesn't work with options binding
}
