namespace Pot.App.Features.Auth.Signup.Complete.Models;

public sealed class Input
{
    public required string Username { get; init; }
    public required string ReferenceCode { get; init; }
    public required string VerificationCode { get; init; }
    public required Guid[] PlatformAdminRowIds { get; init; }
}
