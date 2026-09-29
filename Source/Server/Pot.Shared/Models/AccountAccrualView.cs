namespace Pot.Shared.Models;

/// <summary>
/// The computed accrual state for a single account at a single as-of date.
/// </summary>
public sealed record AccountAccrualView
{
    /// <summary>The total accrual of the cycles in progress.</summary>
    public required double TotalExpenseAccrued { get; init; }

    /// <summary>The total obligation carried for occurrences already past due.</summary>
    public required double TotalArrears { get; init; }

    /// <summary>The headline obligation: <see cref="TotalExpenseAccrued"/> plus <see cref="TotalArrears"/>.</summary>
    public required double TotalCommitted { get; init; }

    /// <summary>The combined daily accrual rate for the cycles in progress.</summary>
    public required double DailyExpenseAccrual { get; init; }

    /// <summary>The combined stable accrual rate.</summary>
    public required double StableExpenseAccrual { get; init; }

    /// <summary>
    /// The per-expense results for every position supplied, or empty when the view came from the totals-only
    /// entry point.
    /// </summary>
    public IReadOnlyList<ExpenseAccrualDetail> Expenses { get; init; } = [];
}
