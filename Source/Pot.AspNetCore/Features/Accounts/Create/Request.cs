using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Request
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

    // Allocated and DailyAccrual are calculated based on linked expenses
}
