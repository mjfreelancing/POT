using Pot.Data.Entities;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class ExpenseRenewalCalculator : IExpenseRenewalCalculator
{
    public void Renew(IEnumerable<ExpenseEntity> expenses, DateOnly advanceUtilDate, bool debitAccount = false)
    {
        foreach (var expense in expenses)
        {
            var endDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);

            if (advanceUtilDate >= endDate)
            {
                continue;
            }

            var nextDue = expense.NextDue;

            while (nextDue < advanceUtilDate)
            {
                var days = expense.Frequency.GetDaysToNext(expense.NextDue, expense.FrequencyCount);
                nextDue = expense.NextDue.AddDays(days);

                // Don't advance beyond the end date
                if (nextDue <= endDate)
                {
                    if (debitAccount)
                    {
                        expense.Account.Balance -= expense.Amount;
                    }

                    // Not resetting / updating expense.Accrued since this impacts the account's accrued amount
                    expense.AccrualStart = expense.NextDue;
                    expense.NextDue = nextDue;
                }
            }
        }
    }
}