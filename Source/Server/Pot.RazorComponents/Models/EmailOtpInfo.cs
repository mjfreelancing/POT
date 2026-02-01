namespace Pot.RazorComponents.Models;

public sealed class EmailOtpInfo : EmailConfigBase
{
    [EmailFormat(EmailFormatType.Both)]
    public required string ReferenceCode { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string VerificationCode { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required int OtpExpiryMinutes { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string TempPassword { get; init; }
}
