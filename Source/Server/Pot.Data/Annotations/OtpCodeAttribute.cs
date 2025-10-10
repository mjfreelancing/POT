using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Annotations;

internal sealed class OtpCodeAttribute : RegularExpressionAttribute
{
    public OtpCodeAttribute() : base(@"^\d{6}$")
    {
        ErrorMessage = "OTP code must be exactly 6 digits";
    }
}