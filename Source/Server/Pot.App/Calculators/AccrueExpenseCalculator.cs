using AllOverIt.Assertion;
using AllOverIt.Extensions;
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

        currentDate ??= _timeProvider.GetLocalDateNow();

        ResetAccountAccruals(account);

        // Sorted will be important if there's ever an option to not allow negative balances.
        // Can't pre-filter since we need to set AccruedIsDirty = false even if the expense is not processed.
        var sortedExpenses = expenses.OrderByDescending(expense => expense.NextDue);

        sortedExpenses.ForEach((expense, index) =>
        {
            expense.Accrued = 0.0d;
            expense.AccruedIsDirty = false;
            expense.LastAccruedUpdate = currentDate.Value;

            var processExpense =
                // The current implementation does not include 'ExcludeFromCalcs' items, but keep here for now so we're not making assumptions.
                !expense.ExcludeFromCalcs &&

                expense.AccrualStart <= currentDate;

            if (processExpense)
            {
                AccrueExpense(currentDate.Value, expense);
            }
        });
    }

    private static void ResetAccountAccruals(AccountEntity account)
    {
        account.TotalExpenseAccrued = 0.0d;
        account.DailyExpenseAccrual = 0.0d;
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
}
