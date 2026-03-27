using AllOverIt.Assertion;
using Pot.App.Concerns.Time;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class AccrueExpenseCalculator : IAccrueExpenseCalculator
{
    private readonly ITimeProvider _timeProvider;

    public AccrueExpenseCalculator(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider.WhenNotNull();
    }

    // When an expense is due on the current date, we assume it has not yet been paid and therefore it should be considered
    // as full accrued but NOT paid. This simplifies the handling of OneTime expenses since once they are paid they should
    // be deleted. All other expenses are implicitly considered paid when they are renewed.
    public void AccrueExpenses(AccountEntity account, IEnumerable<ExpenseEntity> expenses, DateOnly? currentDate = null)
    {
        _ = account.WhenNotNull();
        _ = expenses.WhenNotNull();

        // Using a non-nullable DateOnly variable instead of currentDate ??= pattern to avoid nullable comparison issues.
        // When comparing expense.AccrualStart (DateOnly) with currentDate (DateOnly?), the compiler generates a lifted
        // operator with a null-check branch that cannot be covered since currentDate is guaranteed non-null after this line.
        // By storing in a DateOnly (non-nullable) variable, we eliminate this unreachable null-handling code path.
        var currentDateValue = currentDate ?? _timeProvider.GetLocalDateNow();

        ResetAccountAccruals(account);

        // Sorted will be important if there's ever an option to not allow negative balances.
        // Can't pre-filter since we need to set AccruedIsDirty = false even if the expense is not processed.
        var sortedExpenses = expenses.OrderBy(expense => expense.NextDue);

        foreach (var expense in sortedExpenses)
        {
            expense.Accrued = 0.0d;
            expense.AccruedIsDirty = false;
            expense.LastAccruedUpdate = currentDateValue;

            var processExpense =
                // The current implementation does not include 'ExcludeFromCalcs' items, but keep here for now so we're not making assumptions.
                !expense.ExcludeFromCalcs &&

                expense.AccrualStart <= currentDateValue;

            if (processExpense)
            {
                AccrueExpense(currentDateValue, expense);
                AccrueStableExpense(currentDateValue, expense);
            }
        }
    }

    private static void ResetAccountAccruals(AccountEntity account)
    {
        account.TotalExpenseAccrued = 0.0d;
        account.DailyExpenseAccrual = 0.0d;
        account.StableExpenseAccrual = 0.0d;
    }

    private static void AccrueExpense(DateOnly currentDate, ExpenseEntity expense)
    {
        var account = expense.Account;

        var allocated = currentDate < expense.NextDue
            ? Math.Round(expense.DailyAccrual() * expense.DaysFromAccrualStart(currentDate), 2, MidpointRounding.AwayFromZero)
            : expense.Amount;

        account.TotalExpenseAccrued += allocated;
        expense.Accrued = allocated;

        // Accrual must be applied after the expense allocation has been set
        if (expense.NextDue == currentDate)
        {
            if (expense.Frequency == Frequency.OneTime)
            {
                // One-time expenses will not further accrue
                return;
            }

            // When expenses are renewed, the NextDue date is not set until after they were due. This is to ensure projections are calculated correctly.
            // In this case, when the expense is due on the 'current date' we calculate the daily balance based on the next due date.
            var endDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);
            var daysToNextDue = expense.Frequency.GetDaysToNext(expense.NextDue, expense.FrequencyCount);
            var advancedDate = currentDate.AddDays(daysToNextDue);

            // But only if the expense will be due again.
            if (advancedDate < endDate)
            {
                var dailyAccrual = expense.Amount / daysToNextDue;
                account.DailyExpenseAccrual += dailyAccrual;
            }
        }
        else
        {
            account.DailyExpenseAccrual += expense.DailyBalance(currentDate);
        }
    }

    private static void AccrueStableExpense(DateOnly currentDate, ExpenseEntity expense)
    {
        if (expense.Frequency == Frequency.OneTime)
        {
            // One-time items are treated as a bounded obligation between AccrualStart and NextDue.
            // For the stable metric we intentionally avoid "remaining days" math because that would
            // increase contribution as the due date approaches (unstable ramp-up).
            //
            // Instead we use a fixed denominator for the full configured period:
            //     fixedDays = NextDue - AccrualStart
            //     stable contribution (before due date) = Amount / fixedDays
            //
            // This keeps the one-time contribution constant while active, then zeroes out on/after due date.
            if (currentDate < expense.NextDue)
            {
                var totalDaysUntilDue = expense.NextDue.DayNumber - expense.AccrualStart.DayNumber;

                // Guard malformed or same-day ranges to avoid divide-by-zero or negative periods.
                // A zero/negative period means this one-time item contributes nothing to the stable metric.
                if (totalDaysUntilDue > 0)
                {
                    expense.Account.StableExpenseAccrual += expense.Amount / totalDaysUntilDue;
                }
            }

            // On or after due date: no stable contribution.
            // One-time expenses are expected to be paid and then deleted.
            return;
        }

        // Recurring stable contributions are only active through their end date (if any).
        // Without this gate, expired recurring expenses would continue contributing to
        // StableExpenseAccrual indefinitely unless they are manually excluded or deleted.
        // Keep boundary inclusive so contributions still apply on the end date itself.
        var recurringEndDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);

        if (currentDate > recurringEndDate)
        {
            return;
        }

        var averageDays = expense.Frequency.GetAverageDaysToNext(expense.FrequencyCount);
        expense.Account.StableExpenseAccrual += expense.Amount / averageDays;
    }
}
