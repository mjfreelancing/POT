using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Expenses;

namespace Pot.App.Calculators;

internal sealed class AccrueExpenseCalculator : IAccrueExpenseCalculator
{
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private readonly IPersistableExpenseRepository _expenseRepository;

    public AccrueExpenseCalculator(IPersistableExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
    }

    public async Task AccrueExpensesAsync(Guid accountRowId, CancellationToken cancellationToken)
    {
        using (_expenseRepository.WithTracking())
        {
            var expenses = await _expenseRepository.GetExpensesForAccountAsync(accountRowId, cancellationToken).ConfigureAwait(false);

            // Sorted is important if the option to not allow negative balances is set.
            var sortedExpenses = expenses.OrderByDescending(expense => expense.NextDue);

            sortedExpenses.ForEach((expense, index) =>
            {
                if (index == 0)
                {
                    // All expenses point to the same account
                    ResetAccountAccruals(expense.Account);
                }

                AccrueExpense(expense);
            });

            await _expenseRepository.SaveAsync(cancellationToken);
        }
    }

    private static void ResetAccountAccruals(AccountEntity account)
    {
        account.TotalExpenseAccrued = 0.0d;
        account.DailyExpenseAccrual = 0.0d;
    }

    private void AccrueExpense(ExpenseEntity expense)
    {
        var currentDate = DateOnly.FromDateTime(TimeProvider.GetLocalNow().Date);

        if (expense.AccrualStart > currentDate)
        {
            // If the accrual start date is in the future, no accrual is needed
            return;
        }

        var account = expense.Account;

        var allocated = Math.Round(expense.DailyAccrual() * expense.DaysElapsed(currentDate), 2, MidpointRounding.AwayFromZero);

        // Don't over-allocate
        allocated = Math.Min(allocated, expense.Amount);

        var accountAvailable = account.Available();

        if (/*!calculatorOptions.AllowNegativeBalance &&*/ accountAvailable < allocated)
        {
            allocated = accountAvailable;
        }

        account.TotalExpenseAccrued += allocated;
        expense.Accrued = allocated;

        // Accrual must be applied after the expense allocation has been set
        account.DailyExpenseAccrual += expense.DailyBalance(currentDate);
    }
}
