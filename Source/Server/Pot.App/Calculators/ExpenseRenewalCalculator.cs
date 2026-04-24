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
                    expense.AccrualStart = GetRenewedAccrualStart(expense.AccrualPolicy, asOfDate);
                    expense.NextDue = nextDue;
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
                    var days = expense.Frequency.GetDaysToNext(nextDue, expense.FrequencyCount);
                    var calculatedNextDue = nextDue.AddDays(days);

                    // Don't advance beyond the end date
                    if (calculatedNextDue <= endDate)
                    {
                        // Not resetting / updating expense.Accrued since this impacts the account's accrued amount.
                        // The expense.Accrued will be updated next time the account's 'accrue expenses' is performed.
                        expense.AccrualStart = GetRenewedAccrualStart(expense.AccrualPolicy, nextDue);
                        expense.NextDue = calculatedNextDue;
                        nextDue = calculatedNextDue;
                    }
                    else
                    {
                        // Cannot advance further without exceeding end date, exit loop
                        break;
                    }
                }
            }
        }
    }

    private static DateOnly? GetRenewedAccrualStart(AccrualPolicy accrualPolicy, DateOnly automaticAccrualStart)
    {
        return accrualPolicy switch
        {
            // Using this syntax since AccrualPolicy.Automatic => automaticAccrualStart
            // will not compile because AccrualPolicy.Automatic is not a constant expression (it is a static readonly field).
            var currentPolicy when currentPolicy == AccrualPolicy.Automatic => automaticAccrualStart,
            var currentPolicy when currentPolicy == AccrualPolicy.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(accrualPolicy), accrualPolicy.Name, "Unsupported accrual policy.")
        };
    }
}