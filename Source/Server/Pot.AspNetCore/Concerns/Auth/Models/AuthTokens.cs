namespace Pot.AspNetCore.Concerns.Auth.Models;

/// <summary>Contains the access and refresh tokens issued during authentication.</summary>
/// <param name="AccessToken">The signed access token.</param>
/// <param name="RefreshToken">The token used to renew the authentication session.</param>
/// <param name="RefreshTokenExpiryUtc">The UTC time when the refresh token expires.</param>
public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiryUtc);
