namespace Pot.App.Features.Auth.PasswordReset.Verify.Models;

public sealed class Output
{
    public required OutputStatus Status { get; init; }
    public required string Message { get; init; }
    public int? RetryMinutes { get; init; }
}
