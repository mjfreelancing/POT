using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Default implementation of <see cref="IExpenseRenewalFold"/>.
/// </summary>
internal sealed class ExpenseRenewalFold : IExpenseRenewalFold
{
    /// <inheritdoc />
    public AccrualCursor Renew(ExpenseAccrualInput facts, AccrualCursor cursor, RenewalMode mode, DateOnly asOfDate)
    {
        // Frequency.OneTime expenses do not renew.
        if (facts.ExcludeFromCalcs || facts.Frequency == Frequency.OneTime)
        {
            return cursor;
        }

        var endDate = facts.EndDate.GetValueOrDefault(DateOnly.MaxValue);

        // If the expense has already reached or passed its end date, don't renew.
        if (cursor.NextDue >= endDate)
        {
            return cursor;
        }

        return mode == RenewalMode.Future
            ? RenewFuture(facts, cursor, asOfDate, endDate)
            : RenewOverdue(facts, cursor, asOfDate, endDate);
    }

    // For future items, advance exactly ONCE to the next period.
    private static AccrualCursor RenewFuture(ExpenseAccrualInput facts, AccrualCursor cursor, DateOnly asOfDate, DateOnly endDate)
    {
        var days = facts.Frequency.GetDaysToNext(cursor.NextDue, facts.FrequencyCount);
        var nextDue = cursor.NextDue.AddDays(days);

        // Don't advance beyond the end date.
        if (nextDue > endDate)
        {
            return cursor;
        }

        return new AccrualCursor(nextDue, GetRenewedAccrualStart(facts.AccrualPolicy, asOfDate));
    }

    // Advances the cursor past every occurrence due on or before the as-of date, moving the accrual start forward
    // to the start of each period it passes.
    private static AccrualCursor RenewOverdue(ExpenseAccrualInput facts, AccrualCursor cursor, DateOnly asOfDate, DateOnly endDate)
    {
        var nextDue = cursor.NextDue;
        var accrualStart = cursor.AccrualStart;

        // Advance everything due on or before the asOfDate, including an item due today: RenewalMode.Overdue covers
        // overdue and due-today items alike. A caller that measures the occurrence being settled (the projection
        // loop) must measure it before this runs, otherwise the item is measured against the next period.
        while (nextDue <= asOfDate)
        {
            var days = facts.Frequency.GetDaysToNext(nextDue, facts.FrequencyCount);
            var calculatedNextDue = nextDue.AddDays(days);

            // Cannot advance further without exceeding the end date, so stop.
            if (calculatedNextDue > endDate)
            {
                break;
            }

            accrualStart = GetRenewedAccrualStart(facts.AccrualPolicy, nextDue);
            nextDue = calculatedNextDue;
        }

        return new AccrualCursor(nextDue, accrualStart);
    }

    // Maps the accrual policy to the accrual start of a renewed period: the period boundary for automatic accrual,
    // or null when accrual is disabled.
    private static DateOnly? GetRenewedAccrualStart(AccrualPolicy accrualPolicy, DateOnly automaticAccrualStart)
    {
        return accrualPolicy switch
        {
            // Using this syntax since AccrualPolicy.Automatic => automaticAccrualStart
            // will not compile because AccrualPolicy.Automatic is not a constant expression (it is a static readonly field).
            var currentPolicy when currentPolicy == AccrualPolicy.Automatic => automaticAccrualStart,
            var currentPolicy when currentPolicy == AccrualPolicy.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(accrualPolicy), accrualPolicy.Name, "Unsupported accrual policy.")
        };
    }
}
