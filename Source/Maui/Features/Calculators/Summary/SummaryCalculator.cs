using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Expenses.Models;

namespace Pot.Maui.Features.Calculators.Summary
{
    public static class SummaryCalculator
    {
        public static AccountSummary GetAccountSummary(Account[] accounts)
        {
            var summary = new AccountSummary();

            foreach (var account in accounts)
            {
                summary.Balance += account.Balance;
                summary.Reserved += account.Reserved;
                summary.Allocated += account.Allocated;
                summary.Available += account.Available;
                summary.DailyAccrual += account.DailyAccrual;
            }

            return summary;
        }

        public static ExpenseSummary GetExpenseSummary(Expense[] expenses)
        {
            var summary = new ExpenseSummary();

            foreach (var expense in expenses)
            {
                summary.Total += expense.Amount;
                summary.Allocated += expense.Allocated;
                summary.AccrualDaily += expense.AccrualDaily;
                summary.BalanceDaily += expense.BalanceDaily;
            }

            return summary;
        }
    }
}
