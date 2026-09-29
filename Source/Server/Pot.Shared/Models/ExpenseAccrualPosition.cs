namespace Pot.Shared.Models;

/// <summary>
/// Pairs an expense's immutable accrual facts with the schedule cursor it is measured against.
/// </summary>
/// <param name="Facts">The persisted accrual-relevant facts.</param>
/// <param name="Cursor">The schedule position to measure the facts against.</param>
public readonly record struct ExpenseAccrualPosition(ExpenseAccrualInput Facts, AccrualCursor Cursor)
{
    /// <summary>
    /// Positions the facts on the schedule they were persisted with.
    /// </summary>
    /// <param name="facts">The accrual facts to position.</param>
    /// <returns>The facts measured against their own persisted schedule.</returns>
    /// <remarks>
    /// This is the position every read path uses, because a read measures today. The projection loop positions its
    /// facts on a folded cursor instead, since walking the window advances the schedule as it goes.
    /// </remarks>
    public static ExpenseAccrualPosition AtPersistedSchedule(ExpenseAccrualInput facts)
    {
        return new ExpenseAccrualPosition(facts, new AccrualCursor(facts.NextDue, facts.AccrualStart));
    }
}
