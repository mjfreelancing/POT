namespace Pot.AspNetCore.Concerns.Auth.Models;

/// <summary>Contains request metadata recorded when a user signs in.</summary>
/// <param name="UserAgent">The user-agent value from the request, when available.</param>
/// <param name="IpAddress">The client IP address, when available.</param>
public sealed record LoginRequestContext(string? UserAgent, string? IpAddress);
