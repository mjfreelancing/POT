using AllOverIt.Assertion;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class IncomeRenewalCalculator : IIncomeRenewalCalculator
{
    // advanceUntilDate is typically 'today' (except when calculating projections)
    public void Renew(IEnumerable<IncomeEntity> incomes, DateOnly advanceUntilDate)
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

            var nextDue = income.NextDue;

            // Do not process items due on the advanceUtilDate - theoretically 'still due' and it would affect how projections
            // are calculated because the expenses would continue to advance before the credit could be considered.
            while (nextDue <= advanceUntilDate)
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
