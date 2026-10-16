using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Verify;

/// <summary>Carries the username, reference code and verification code needed to verify a password-reset request.</summary>
public sealed class Request
{
    /// <summary>The user's username</summary>
    [Description("The user's username")]
    public required string Username { get; set; }

    /// <summary>The reference code to pair with the OTP verification code</summary>
    [Description("The reference code to pair with the OTP verification code")]
    public required string ReferenceCode { get; set; }

    /// <summary>The OTP verification code</summary>
    [Description("The OTP verification code")]
    public required string VerificationCode { get; set; }
}
