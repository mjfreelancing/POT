namespace Pot.App.Features.Auth.Signup.Complete.Models;

public sealed class Output
{
    public required OutputStatus Status { get; init; }
    public required string Message { get; init; }
    public int? RetryMinutes { get; init; }
}
