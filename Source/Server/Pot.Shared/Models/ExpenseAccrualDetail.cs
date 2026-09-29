namespace Pot.Shared.Models;

/// <summary>
/// The accrual result for a single expense.
/// </summary>
public sealed record ExpenseAccrualDetail
{
    /// <summary>The expense identity the result belongs to.</summary>
    public required Guid RowId { get; init; }

    /// <summary>The accrual of the cycle in progress, bounded by the expense amount.</summary>
    public required double Accrued { get; init; }

    /// <summary>The obligation carried for occurrences already past due.</summary>
    public required double Arrears { get; init; }
}
