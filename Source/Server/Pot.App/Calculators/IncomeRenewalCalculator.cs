using AllOverIt.Assertion;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class IncomeRenewalCalculator : IIncomeRenewalCalculator
{
    // asOfDate is typically 'today' (except when calculating projections)
    public void Renew(IEnumerable<IncomeEntity> incomes, RenewalMode mode, DateOnly asOfDate)
    {
        _ = incomes.WhenNotNull();

        foreach (var income in incomes)
        {
            // Frequency.OneTime incomes do not renew
            if (income.ExcludeFromCalcs || income.Frequency == Frequency.OneTime)
            {
                continue;
            }

            var endDate = income.EndDate.GetValueOrDefault(DateOnly.MaxValue);

            // If the income has already reached or passed its end date, don't renew
            if (income.NextDue >= endDate)
            {
                continue;
            }

            if (mode == RenewalMode.Future)
            {
                // For future items, advance exactly ONCE to the next period
                var days = income.Frequency.GetDaysToNext(income.NextDue, income.FrequencyCount);
                var nextDue = income.NextDue.AddDays(days);

                // Don't advance beyond the end date
                if (nextDue <= endDate)
                {
                    income.NextDue = nextDue;
                }
            }
            else
            {
                // For overdue mode, advance until caught up (existing logic)
                var nextDue = income.NextDue;

                // Do not process items due on the asOfDate - theoretically 'still due' and it would affect how projections
                // are calculated because the income would continue to advance before the credit could be considered.
                while (nextDue <= asOfDate)
                {
                    var days = income.Frequency.GetDaysToNext(income.NextDue, income.FrequencyCount);
                    nextDue = income.NextDue.AddDays(days);

                    if (nextDue <= endDate)
                    {
                        income.NextDue = nextDue;
                    }
                }
            }
        }
    }
}
