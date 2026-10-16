namespace Pot.AspNetCore.Concerns.Auth.Configuration;

// Configuration options for authentication features including cookie settings.
// Bound from appsettings.json Authentication section.
/// <summary>Provides settings for API authentication and refresh-token cookies.</summary>
public sealed class AuthenticationOptions
{
    private const string RefreshTokenName = "pot_refresh_token";

    /// <summary>Provides settings for the refresh-token cookie.</summary>
    public sealed class CookieOptions
    {
        // The name of the refresh token cookie.
        /// <summary>Gets the name of the refresh-token cookie.</summary>
        public string Name => RefreshTokenName;

        // When true, cookies are only sent over HTTPS connections (required for production).
        // Set to false for local HTTP development.
        /// <summary>Gets or sets whether the cookie is restricted to secure HTTPS requests.</summary>
        public bool SecureOnly { get; set; } = true;

        // Cookie lifetime in days.
        /// <summary>Gets or sets the maximum age of the refresh-token cookie in days.</summary>
        public int MaxAgeDays { get; set; } = 30;

        // The domain scope for the cookie. Use ".domain.com" for cross-subdomain cookies.
        // Leave null for same-origin cookies (local development).
        /// <summary>Gets or sets the domain associated with the refresh-token cookie.</summary>
        public string? Domain { get; set; }
    }

    // The refresh token cookie-specific authentication settings.
    /// <summary>Gets or sets the refresh-token cookie settings.</summary>
    public CookieOptions Cookie { get; set; } = null!;      // Cannot use required - doesn't work with options binding
}
