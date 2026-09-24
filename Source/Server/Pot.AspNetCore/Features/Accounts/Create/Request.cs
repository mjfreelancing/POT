using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Request
{
    [Description("A description of the account")]
    public string Description { get; init; } = string.Empty;

    [Description("The account balance")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount")]
    public double Reserved { get; init; }

    // TotalExpenseAccrued and DailyExpenseAccrual are calculated based on linked expenses
}
