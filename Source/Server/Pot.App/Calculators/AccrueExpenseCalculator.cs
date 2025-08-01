using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Pot.App.Concerns.Time;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class AccrueExpenseCalculator : IAccrueExpenseCalculator
{
    private readonly ITimeProvider _timeProvider;

    public AccrueExpenseCalculator(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider.WhenNotNull();
    }

    public void AccrueExpenses(AccountEntity account, IEnumerable<ExpenseEntity> expenses, DateOnly? currentDate = null)
    {
        currentDate ??= _timeProvider.GetLocalDateNow();

        ResetAccountAccruals(account);

        // Sorted is important if the option to not allow negative balances is set.
        var sortedExpenses = expenses.OrderByDescending(expense => expense.NextDue);

        sortedExpenses.ForEach((expense, index) =>
        {
            AccrueExpense(expense, currentDate.Value);
        });
    }

    private static void ResetAccountAccruals(AccountEntity account)
    {
        account.TotalExpenseAccrued = 0.0d;
        account.DailyExpenseAccrual = 0.0d;
    }

    private static void AccrueExpense(ExpenseEntity expense, DateOnly currentDate)
    {
        expense.Accrued = 0.0d;

        // TODO: Create a composite specification for these early returns

        // If the accrual start date is in the future, no accrual is needed.
        if (expense.AccrualStart > currentDate)
        {
            return;
        }

        // If the expense was a one-time expense and the current date is after the next due date, no accrual is needed.
        if (expense.Frequency == Shared.Frequency.OneTime && currentDate > expense.NextDue)
        {
            return;
        }

        var account = expense.Account;

        var allocated = Math.Round(expense.DailyAccrual() * expense.DaysFromAccrualStart(currentDate), 2, MidpointRounding.AwayFromZero);

        // TODO: Can this ever occur? May be precision related issues?
        // Don't over-allocate
        allocated = Math.Min(allocated, expense.Amount);

        account.TotalExpenseAccrued += allocated;
        expense.Accrued = allocated;

        // Accrual must be applied after the expense allocation has been set
        if (expense.NextDue == currentDate)
        {
            if (expense.Frequency == Shared.Frequency.OneTime)
            {
                // One-time expenses will not further accrue
                return;
            }

            // When expenses are renewed, the NextDue date is not set until after they were due. This is to ensure projections are calculated correctly.
            // In this case, when the expense is due on the 'current date' we calculate the daily balance based on the next due date.
            var endDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);
            var days = expense.Frequency.GetDaysToNext(expense.NextDue, expense.FrequencyCount);
            var advancedDate = currentDate.AddDays(days);

            // But only if the expense will be due again.
            if (advancedDate < endDate)
            {
                var dailyAccrual = expense.Amount / Math.Max(days, 1);
                account.DailyExpenseAccrual += dailyAccrual;
            }
        }
        else
        {
            account.DailyExpenseAccrual += expense.DailyBalance(currentDate);
        }
    }
}
