using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Create;

/// <summary>Carries the values needed to create an account.</summary>
public sealed class Request
{
    /// <summary>A description of the account</summary>
    [Description("A description of the account")]
    public string Description { get; init; } = string.Empty;

    /// <summary>The account balance</summary>
    [Description("The account balance")]
    public double Balance { get; init; }

    /// <summary>The minimum reserved amount</summary>
    [Description("The minimum reserved amount")]
    public double Reserved { get; init; }

    // TotalExpenseAccrued and DailyExpenseAccrual are calculated based on linked expenses
}
