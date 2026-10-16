namespace Pot.AspNetCore.Concerns.Auth.Models;

/// <summary>Provides the issuer, audience and signing key used for JWT access tokens.</summary>
public sealed class JwtOptions
{
    /// <summary>Gets or initializes the issuer used in JWT access tokens.</summary>
    public required string Issuer { get; init; }
    /// <summary>Gets or initializes the audience used in JWT access tokens.</summary>
    public required string Audience { get; init; }
    /// <summary>Gets or initializes the key used to sign JWT access tokens.</summary>
    public required string SecretKey { get; init; }
}