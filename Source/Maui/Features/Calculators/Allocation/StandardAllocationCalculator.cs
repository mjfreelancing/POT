using AllOverIt.Extensions;
using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Expenses.Models;

namespace Pot.Maui.Features.Calculators.Allocation
{
    internal sealed class StandardAllocationCalculator : IAllocationCalculator
    {
        public void AllocateFunds(Account[] accounts, Expense[] expenses, AllocationCalculatorSettings calculatorSettings)
        {
            ReleaseAllocations(accounts, expenses);

            // Process in order of 'days due' to best cater for accounts with insufficient funds
            var orderedExpenses = expenses.OrderBy(expense => expense.DaysDue);

            foreach (var expense in orderedExpenses)
            {
                var account = accounts.Single(item => item.Id == expense.AccountId);

                // expense.DaysElapsed takes into account if the expense is due beyond the usual expense period
                var allocated = Math.Round(expense.AccrualDaily * expense.DaysElapsed, 2, MidpointRounding.AwayFromZero);

                // Don't over-allocate
                allocated = Math.Min(allocated, expense.Amount);

                if (!calculatorSettings.AllowNegativeBalance && account.Available < allocated)
                {
                    allocated = account.Available;
                }

                // adjust allocated and accrual amounts
                account.Allocated += allocated;
                expense.Allocated = allocated;

                // Accrual must be applied after the expense allocation has been set
                account.DailyAccrual += expense.BalanceDaily;
            }
        }

        private static void ReleaseAllocations(Account[] accounts, Expense[] expenses)
        {
            expenses
              .Select(expense => expense.AccountId)
              .Distinct()
              .ForEach((accountId, _) =>
              {
                  var account = accounts.Single(item => item.Id == accountId);

                  account.Allocated = 0.0d;
                  account.DailyAccrual = 0.0d;
              });
        }
    }
}
