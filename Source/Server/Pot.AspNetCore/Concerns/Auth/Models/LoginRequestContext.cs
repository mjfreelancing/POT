namespace Pot.AspNetCore.Concerns.Auth.Models;

public sealed record LoginRequestContext(string? UserAgent, string? IpAddress);
