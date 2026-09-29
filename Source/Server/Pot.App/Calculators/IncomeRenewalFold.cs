using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Default implementation of <see cref="IIncomeRenewalFold"/>.
/// </summary>
internal sealed class IncomeRenewalFold : IIncomeRenewalFold
{
    /// <inheritdoc />
    public IncomeScheduleInput Renew(IncomeScheduleInput schedule, RenewalMode mode, DateOnly asOfDate)
    {
        // Frequency.OneTime incomes do not renew.
        if (schedule.ExcludeFromCalcs || schedule.Frequency == Frequency.OneTime)
        {
            return schedule;
        }

        var endDate = schedule.EndDate.GetValueOrDefault(DateOnly.MaxValue);

        // If the income has already reached or passed its end date, don't renew.
        if (schedule.NextDue >= endDate)
        {
            return schedule;
        }

        return mode == RenewalMode.Future
            ? RenewFuture(schedule, endDate)
            : RenewOverdue(schedule, asOfDate, endDate);
    }

    // For future items, advance exactly ONCE to the next period.
    private static IncomeScheduleInput RenewFuture(IncomeScheduleInput schedule, DateOnly endDate)
    {
        var days = schedule.Frequency.GetDaysToNext(schedule.NextDue, schedule.FrequencyCount);
        var nextDue = schedule.NextDue.AddDays(days);

        // Don't advance beyond the end date.
        return nextDue > endDate ? schedule : schedule with { NextDue = nextDue };
    }

    // Advances the cursor past every occurrence due on or before the as-of date.
    private static IncomeScheduleInput RenewOverdue(IncomeScheduleInput schedule, DateOnly asOfDate, DateOnly endDate)
    {
        var nextDue = schedule.NextDue;

        // Advance everything due on or before the asOfDate, including an item due today: RenewalMode.Overdue covers
        // overdue and due-today items alike. A caller that measures the occurrence being settled (the projection
        // loop) must measure it before this runs, otherwise the item is measured against the next period.
        while (nextDue <= asOfDate)
        {
            var days = schedule.Frequency.GetDaysToNext(nextDue, schedule.FrequencyCount);
            var calculatedNextDue = nextDue.AddDays(days);

            // Cannot advance further without exceeding the end date, so stop.
            if (calculatedNextDue > endDate)
            {
                break;
            }

            nextDue = calculatedNextDue;
        }

        return schedule with { NextDue = nextDue };
    }
}
