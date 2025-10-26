namespace Pot.App.Features.Auth.Signup.Request.Models;

public sealed class Input
{
    public required string Username { get; init; }
    public required string Email { get; set; }
    public required string CorrelationId { get; init; }
}
