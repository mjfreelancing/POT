using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Signup.Complete;

public sealed class Request
{
    [Description("The user's username")]
    public required string Username { get; set; }

    [Description("The reference code to pair with the OTP verification code")]
    public required string ReferenceCode { get; set; }

    [Description("The OTP verficaition code")]
    public required string VerificationCode { get; set; }
}
