using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Signup.Complete;

/// <summary>Carries the username, reference code and verification code needed to complete user registration.</summary>
public sealed class Request
{
    /// <summary>The user's username</summary>
    [Description("The user's username")]
    public required string Username { get; set; }

    /// <summary>The reference code to pair with the OTP verification code</summary>
    [Description("The reference code to pair with the OTP verification code")]
    public required string ReferenceCode { get; set; }

    /// <summary>The OTP verficaition code</summary>
    [Description("The OTP verficaition code")]
    public required string VerificationCode { get; set; }
}
