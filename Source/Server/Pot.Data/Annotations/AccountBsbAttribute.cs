using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Annotations;

internal sealed class AccountBsbAttribute : RegularExpressionAttribute
{
    public AccountBsbAttribute() : base(@"^\d{3}-\d{3}$")
    {
        ErrorMessage = "Account BSB must be in the format ###-###";
    }
}
