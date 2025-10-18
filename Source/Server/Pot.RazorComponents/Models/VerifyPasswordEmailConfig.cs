namespace Pot.RazorComponents.Models;

public sealed class VerifyPasswordEmailConfig : EmailConfigBase
{
    public required string ReferenceCode { get; init; }
    public required string VerificationCode { get; init; }
    public required int OtpExpiryMinutes { get; init; }
    public required string TempPassword { get; init; }
}