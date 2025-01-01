using Pot.AspNetCore.Features.Calculators.Models;
using Pot.Data.Dtos;

namespace Pot.AspNetCore.Features.Calculators.Summary
{
    public static class SummaryCalculator
    {
        public static AccountSummary GetAccountSummary(AccountDto[] accounts)
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

        public static ExpenseSummary GetExpenseSummary(ExpenseDto[] expenses)
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
