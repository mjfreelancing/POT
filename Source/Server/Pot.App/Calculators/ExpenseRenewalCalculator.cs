using AllOverIt.Assertion;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class ExpenseRenewalCalculator : IExpenseRenewalCalculator
{
    // asOfDate is typically 'today' (except when calculating projections)
    public void Renew(IEnumerable<ExpenseEntity> expenses, RenewalMode mode, DateOnly asOfDate)
    {
        _ = expenses.WhenNotNull();

        foreach (var expense in expenses)
        {
            // Leave expense.AccruedIsDirty in its current state - if it was dirty then the accruals should still be updated

            // Frequency.OneTime expenses do not renew
            if (expense.ExcludeFromCalcs || expense.Frequency == Frequency.OneTime)
            {
                continue;
            }

            var endDate = expense.EndDate.GetValueOrDefault(DateOnly.MaxValue);

            // If the expense has already reached or passed its end date, don't renew
            if (expense.NextDue >= endDate)
            {
                continue;
            }

            if (mode == RenewalMode.Future)
            {
                // For future items, advance exactly ONCE to the next period
                var days = expense.Frequency.GetDaysToNext(expense.NextDue, expense.FrequencyCount);
                var nextDue = expense.NextDue.AddDays(days);

                // Don't advance beyond the end date
                if (nextDue <= endDate)
                {
                    // Not resetting / updating expense.Accrued since this impacts the account's accrued amount.
                    // The expense.Accrued will be updated next time the account's 'accrue expenses' is performed.
                    expense.AccrualStart = expense.NextDue;
                    expense.NextDue = nextDue;
                    expense.AccruedIsDirty = true;
                }
            }
            else
            {
                // For overdue mode, advance until caught up (existing logic)
                var nextDue = expense.NextDue;

                // Do not process items due on the asOfDate - theoretically 'still due' and it would affect how projections
                // are calculated because the expenses would continue to advance before the debit could be considered.
                while (nextDue <= asOfDate)
                {
                    var days = expense.Frequency.GetDaysToNext(expense.NextDue, expense.FrequencyCount);
                    nextDue = expense.NextDue.AddDays(days);

                    // Don't advance beyond the end date
                    if (nextDue <= endDate)
                    {
                        // Not resetting / updating expense.Accrued since this impacts the account's accrued amount.
                        // The expense.Accrued will be updated next time the account's 'accrue expenses' is performed.
                        expense.AccrualStart = expense.NextDue;
                        expense.NextDue = nextDue;
                        expense.AccruedIsDirty = true;
                    }
                }
            }
        }
    }
}