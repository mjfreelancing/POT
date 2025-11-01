namespace Pot.App.Features.Otp.Models;

public sealed class UserOtpData
{
    public required string TempPassword { get; init; }
    public required string TempPasswordHash { get; init; }
    public required string ReferenceCode { get; init; }
    public required string OtpCode { get; init; }
    public required int OtpExpiryMinutes { get; init; }

    public static readonly UserOtpData None = new()
    {
        TempPassword = string.Empty,
        TempPasswordHash = string.Empty,
        ReferenceCode = string.Empty,
        OtpCode = string.Empty,
        OtpExpiryMinutes = 0
    };
}
