namespace Pot.Shared.Models;

/// <summary>
/// The schedule position an expense is measured against.
/// </summary>
/// <param name="NextDue">The next due date at the measured point in time.</param>
/// <param name="AccrualStart">The accrual start date at the measured point in time, if any.</param>
/// <remarks>
/// A value type, so the projection loop can fold the schedule forward day by day without allocating and without
/// mutating the immutable <see cref="ExpenseAccrualInput"/> it belongs to.
/// </remarks>
public readonly record struct AccrualCursor(DateOnly NextDue, DateOnly? AccrualStart);
