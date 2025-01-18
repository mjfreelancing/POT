using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class Request
{
    [Description("The account BSB.")]
    public string Bsb { get; init; } = string.Empty;

    [Description("The account number.")]
    public string Number { get; init; } = string.Empty;

    [Description("A description of the account.")]
    public string Description { get; init; } = string.Empty;

    [Description("The account balance.")]
    public double Balance { get; init; }

    [Description("The minimum reserved amount.")]
    public double Reserved { get; init; }

    [Description("The amount allocated to future expenses.")]
    public double Allocated { get; init; }

    [Description("The daily accrual required to meet all future expenses.")]
    public double DailyAccrual { get; init; }
}
