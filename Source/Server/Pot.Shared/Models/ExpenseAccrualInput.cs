using Pot.Shared.Enumerations;

namespace Pot.Shared.Models;

/// <summary>
/// The accrual-relevant facts for a single expense.
/// </summary>
/// <remarks>
/// The schedule members are the persisted values. The accrual calculation measures the expense against an
/// <see cref="AccrualCursor"/> supplied alongside these facts, which lets the projection loop fold the schedule
/// forward without mutating this record or the entity it was projected from.
/// </remarks>
public sealed record ExpenseAccrualInput
{
    /// <summary>The expense identity, so per-expense results can be attributed back to their row.</summary>
    public required Guid RowId { get; init; }

    /// <summary>Whether the expense is excluded from calculations.</summary>
    public required bool ExcludeFromCalcs { get; init; }

    /// <summary>The persisted accrual start date, if any.</summary>
    public required DateOnly? AccrualStart { get; init; }

    /// <summary>The persisted next due date.</summary>
    public required DateOnly NextDue { get; init; }

    /// <summary>The persisted end date, if any.</summary>
    public required DateOnly? EndDate { get; init; }

    /// <summary>The accrual policy that governs whether and from when the expense accrues.</summary>
    public required AccrualPolicy AccrualPolicy { get; init; }

    /// <summary>The recurrence frequency.</summary>
    public required Frequency Frequency { get; init; }

    /// <summary>The number of frequency units between occurrences.</summary>
    public required int FrequencyCount { get; init; }

    /// <summary>The expense amount for a single occurrence.</summary>
    public required double Amount { get; init; }
}
