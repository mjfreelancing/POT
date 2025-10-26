namespace Pot.App.Features.Auth.Signup.Request.Models;

public sealed class Output
{
    public required OutputStatus Status { get; init; }
    public required string Message { get; init; }
    public string? ReferenceCode { get; init; }             // Will be null when cannot sign up due to username taken
}
