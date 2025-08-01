using Pot.Data.Entities;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class ExpenseRenewalCalculator : IExpenseRenewalCalculator
{
    public void Renew(IEnumerable<ExpenseEntity> expenses, DateOnly advanceUntilDate, bool debitAccount = false)
    {
        foreach (var expense in expenses)
        {
            if (expense.Frequency == Shared.Frequency.OneTime)
            {
                // One-time expenses do not renew
                continue;
            }

            var endDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);

            if (advanceUntilDate >= endDate)
            {
                continue;
            }

            var nextDue = expense.NextDue;

            // Do not process items due on the advanceUntilDate - theoretically 'still due' and it would affect how projections
            // are calculated because the expenses would continue to advance before the debit could be considered.
            while (nextDue < advanceUntilDate)
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