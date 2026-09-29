using Pot.Shared.Enumerations;

namespace Pot.Shared.Models;

/// <summary>
/// The schedule facts for a single income.
/// </summary>
/// <remarks>
/// An income has no accrual state, so this record carries only what renewal needs. Its <see cref="NextDue"/> is
/// also the fold's cursor: renewal returns a copy with the schedule advanced.
/// </remarks>
public sealed record IncomeScheduleInput
{
    /// <summary>The identity of the income the schedule belongs to, so a folded result can be attributed back.</summary>
    public required Guid RowId { get; init; }

    /// <summary>The next due date at the measured point in time.</summary>
    public required DateOnly NextDue { get; init; }

    /// <summary>The persisted end date, if any.</summary>
    public required DateOnly? EndDate { get; init; }

    /// <summary>The recurrence frequency.</summary>
    public required Frequency Frequency { get; init; }

    /// <summary>The number of frequency units between occurrences.</summary>
    public required int FrequencyCount { get; init; }

    /// <summary>Whether the income is excluded from calculations.</summary>
    public required bool ExcludeFromCalcs { get; init; }
}
