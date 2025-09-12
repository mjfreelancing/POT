namespace Pot.AspNetCore.Concerns.Auth.Models;

public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiryUtc);
