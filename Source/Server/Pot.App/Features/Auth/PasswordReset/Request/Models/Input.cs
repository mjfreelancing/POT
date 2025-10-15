namespace Pot.App.Features.Auth.PasswordReset.Request.Models;

public sealed class Input
{
    public required string Username { get; init; }
    public required string CorrelationId { get; init; }
}