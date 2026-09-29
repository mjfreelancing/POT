using AllOverIt.Assertion;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;
using Pot.Shared.Models;

namespace Pot.App.Calculators;

/// <summary>
/// Default implementation of <see cref="IAccrualCalculator"/>.
/// </summary>
/// <remarks>
/// The calculation is stateless: both entry points delegate to static helpers, so the same positions and as-of date
/// always produce the same result and nothing beyond the supplied arguments is read or written.
///
/// An account's aggregates are the sum of its per-expense results. An expense flagged as excluded is not filtered
/// out; it contributes a zeroed result so its detail entry is still produced.
///
/// Monetary results are rounded to two decimal places, away from zero.
/// </remarks>
internal sealed class AccrualCalculator : IAccrualCalculator
{
    // The position of the cycle in progress, plus the obligation accumulated for occurrences already past due.
    private readonly record struct ScheduleWalk(DateOnly NextDue, DateOnly? AccrualStart, bool HasCycleInProgress, double Arrears);

    // The accrual outcome for a single expense.
    private readonly record struct ExpenseAccrualResult(double Accrued, double Arrears, double DailyAccrual, double StableAccrual)
    {
        public static ExpenseAccrualResult Zero { get; } = new(0.0d, 0.0d, 0.0d, 0.0d);
    }

    private const int AccrualRoundingDecimals = 2;

    /// <inheritdoc />
    public AccountAccrualView CalculateAccountTotals(IEnumerable<ExpenseAccrualPosition> positions, DateOnly asOfDate)
    {
        _ = positions.WhenNotNull();

        return Calculate(positions, asOfDate, includeExpenseDetail: false);
    }

    /// <inheritdoc />
    public AccountAccrualView CalculateAccountWithExpenseDetail(IEnumerable<ExpenseAccrualPosition> positions, DateOnly asOfDate)
    {
        _ = positions.WhenNotNull();

        return Calculate(positions, asOfDate, includeExpenseDetail: true);
    }

    // The shared aggregation behind both entry points: it walks the positions once, delegating each to
    // CalculateExpense, and accumulates the four account-level totals. includeExpenseDetail only controls whether a
    // per-expense detail entry is collected alongside the totals, so the totals themselves are computed by a single
    // piece of logic rather than duplicated for each entry point.
    //
    // The detail list is created only when it is asked for, so the totals-only path allocates nothing per row:
    // details is null and details?.Add skips the work without a branch inside the loop.
    //
    // A detail entry carries only the identity and the accrued/arrears pair, because the daily and stable rates are
    // rates for the account as a whole rather than values the caller attributes to an individual row. TotalCommitted
    // is derived here rather than supplied, so it can never drift from the two totals it is made of.
    //
    // When no detail is requested, Expenses is set to an empty list rather than null, so callers can enumerate the
    // result unconditionally.
    private static AccountAccrualView Calculate(IEnumerable<ExpenseAccrualPosition> positions, DateOnly asOfDate,
        bool includeExpenseDetail)
    {
        var totalExpenseAccrued = 0.0d;
        var totalArrears = 0.0d;
        var dailyExpenseAccrual = 0.0d;
        var stableExpenseAccrual = 0.0d;
        var details = includeExpenseDetail ? new List<ExpenseAccrualDetail>() : null;

        foreach (var position in positions)
        {
            var result = CalculateExpense(position, asOfDate);

            totalExpenseAccrued += result.Accrued;
            totalArrears += result.Arrears;
            dailyExpenseAccrual += result.DailyAccrual;
            stableExpenseAccrual += result.StableAccrual;

            details?.Add(new ExpenseAccrualDetail
            {
                RowId = position.Facts.RowId,
                Accrued = result.Accrued,
                Arrears = result.Arrears
            });
        }

        return new AccountAccrualView
        {
            TotalExpenseAccrued = totalExpenseAccrued,
            TotalArrears = totalArrears,
            TotalCommitted = totalExpenseAccrued + totalArrears,
            DailyExpenseAccrual = dailyExpenseAccrual,
            StableExpenseAccrual = stableExpenseAccrual,
            Expenses = details ?? []
        };
    }

    // Computes the accrual outcome for a single expense: the arrears already past due, the accrual ramped within the
    // cycle in progress, and the daily and stable rates that describe it.
    private static ExpenseAccrualResult CalculateExpense(ExpenseAccrualPosition position, DateOnly asOfDate)
    {
        var facts = position.Facts;
        var cursor = position.Cursor;

        // Exclusion is the opt-out: the row is absent from the aggregate and contributes nothing to its own detail.
        // The accounts and projection queries already filter excluded rows out; keeping the guard here makes the
        // calculation correct on its own terms rather than dependent on how it was called.
        if (facts.ExcludeFromCalcs)
        {
            return ExpenseAccrualResult.Zero;
        }

        var schedule = WalkSchedule(facts, cursor, asOfDate);

        // Whether the row accrues at all is decided on the start-of-day cursor, exactly as the persisted rule did:
        // a null accrual start is not a start of zero, and a row whose accrual has not begun contributes nothing.
        if (!ShouldAccrue(facts, cursor, asOfDate))
        {
            return new ExpenseAccrualResult(0.0d, schedule.Arrears, 0.0d, 0.0d);
        }

        // The stable metric is an average rate rather than an obligation, so it is measured on the start-of-day
        // cursor. This is deliberately independent of the cycle in progress: an ended recurring row still
        // contributes up to its end date, which is what the persisted rule did.
        var stableAccrual = CalculateStableAccrual(facts, cursor, asOfDate);

        if (!schedule.HasCycleInProgress)
        {
            // The schedule has nothing left to accrue towards, so the obligation sits entirely in arrears.
            return new ExpenseAccrualResult(0.0d, schedule.Arrears, 0.0d, stableAccrual);
        }

        var accrued = CalculateAccrued(facts, schedule.NextDue, schedule.AccrualStart, asOfDate);
        var dailyAccrual = CalculateDailyAccrual(facts, schedule.NextDue, accrued, asOfDate);

        return new ExpenseAccrualResult(accrued, schedule.Arrears, dailyAccrual, stableAccrual);
    }

    // Whether the expense accrues at all, decided on the start-of-day cursor.
    //
    // An AccrualPolicy.None row never accrues, and a null accrual start is deliberately not treated as an accrual
    // start of zero: the persisted rule leaves both unprocessed. The null case is reachable only through import or
    // legacy data, because create and update canonicalise the value.
    private static bool ShouldAccrue(ExpenseAccrualInput facts, AccrualCursor cursor, DateOnly asOfDate)
    {
        if (facts.AccrualPolicy == AccrualPolicy.None)
        {
            return false;
        }

        if (!cursor.AccrualStart.HasValue)
        {
            return false;
        }

        return cursor.AccrualStart.Value <= asOfDate;
    }

    // Walks the schedule forward from the cursor, separating occurrences that are already past due from the cycle
    // in progress.
    private static ScheduleWalk WalkSchedule(ExpenseAccrualInput facts, AccrualCursor cursor, DateOnly asOfDate)
    {
        var endDate = facts.EndDate.GetValueOrDefault(DateOnly.MaxValue);

        if (facts.Frequency == Frequency.OneTime)
        {
            // A one-time expense does not renew, so it carries at most the single occurrence.
            var isPastDue = cursor.NextDue < asOfDate;
            var arrears = isPastDue && cursor.NextDue <= endDate ? facts.Amount : 0.0d;

            return new ScheduleWalk(cursor.NextDue, cursor.AccrualStart, HasCycleInProgress: !isPastDue, arrears);
        }

        var cycleNextDue = cursor.NextDue;
        var cycleAccrualStart = cursor.AccrualStart;
        var totalArrears = 0.0d;

        // An occurrence is arrears when its due date is strictly before the as-of date. A bill due today is the
        // current bill: it accrues in full and is not arrears, which is what stops it being counted twice.
        while (cycleNextDue < asOfDate)
        {
            if (cycleNextDue <= endDate)
            {
                totalArrears += facts.Amount;
            }

            var advancedDue = cycleNextDue.AddDays(facts.Frequency.GetDaysToNext(cycleNextDue, facts.FrequencyCount));

            // Stopping when the recurrence cannot move forward guards malformed data (a zero frequency count)
            // against an unbounded walk; the end-date clamp stops occurrences beyond the end date.
            if (advancedDue <= cycleNextDue || advancedDue > endDate)
            {
                return new ScheduleWalk(cycleNextDue, cycleAccrualStart, HasCycleInProgress: false, totalArrears);
            }

            cycleAccrualStart = cycleNextDue;
            cycleNextDue = advancedDue;
        }

        return new ScheduleWalk(cycleNextDue, cycleAccrualStart, HasCycleInProgress: true, totalArrears);
    }

    // Returns the accrual of the cycle in progress: the full amount on its due date, otherwise the accrual ramped
    // from the accrual start and bounded by the amount.
    private static double CalculateAccrued(ExpenseAccrualInput facts, DateOnly cycleNextDue, DateOnly? cycleAccrualStart, DateOnly asOfDate)
    {
        if (asOfDate >= cycleNextDue)
        {
            return facts.Amount;
        }

        // A null accrual start cannot reach here (the accrual gate already requires one); defaulting to the due
        // date keeps the arithmetic total rather than relying on a null-forgiving dereference.
        var accrualStart = cycleAccrualStart.GetValueOrDefault(cycleNextDue);
        var accrualDays = Math.Max(accrualStart.DaysUntil(cycleNextDue), 1);
        var daysFromAccrualStart = accrualStart.DaysUntil(asOfDate);

        return Math.Round(facts.Amount / accrualDays * daysFromAccrualStart, AccrualRoundingDecimals, MidpointRounding.AwayFromZero);
    }

    // Returns the daily accrual rate for the cycle in progress.
    private static double CalculateDailyAccrual(ExpenseAccrualInput facts, DateOnly cycleNextDue, double accrued, DateOnly asOfDate)
    {
        if (cycleNextDue == asOfDate)
        {
            if (facts.Frequency == Frequency.OneTime)
            {
                return 0.0d;
            }

            var endDate = facts.EndDate.GetValueOrDefault(DateOnly.MaxValue);
            var daysToNextDue = facts.Frequency.GetDaysToNext(cycleNextDue, facts.FrequencyCount);

            // Only accrue onwards when the expense will actually be due again.
            return cycleNextDue.AddDays(daysToNextDue) < endDate ? facts.Amount / daysToNextDue : 0.0d;
        }

        // The remaining obligation for the cycle in progress, spread over the days until it falls due.
        return (facts.Amount - accrued) / Math.Max(asOfDate.DaysUntil(cycleNextDue), 1);
    }

    // Returns the stable accrual rate, which is a fixed contribution per configured period rather than a ramp.
    private static double CalculateStableAccrual(ExpenseAccrualInput facts, AccrualCursor cursor, DateOnly asOfDate)
    {
        if (facts.Frequency == Frequency.OneTime)
        {
            // A one-time expense uses a fixed denominator for its full configured period, so its contribution is
            // constant while active and zero on or after its due date.
            if (asOfDate >= cursor.NextDue || !cursor.AccrualStart.HasValue)
            {
                return 0.0d;
            }

            var totalDaysUntilDue = cursor.NextDue.DayNumber - cursor.AccrualStart.Value.DayNumber;

            // The zero/negative period guard is retained from the persisted rule as a division guard. The accrual
            // gate above already guarantees a positive period, so it is unreachable rather than a behaviour branch,
            // and it is kept because a negative or zero denominator would silently produce a nonsensical rate.
            return totalDaysUntilDue > 0 ? facts.Amount / totalDaysUntilDue : 0.0d;
        }

        // Recurring contributions are gated by the end date, kept inclusive so a row still contributes on the
        // end date itself.
        return asOfDate <= facts.EndDate.GetValueOrDefault(DateOnly.MaxValue)
            ? facts.Amount / facts.Frequency.GetAverageDaysToNext(facts.FrequencyCount)
            : 0.0d;
    }
}
