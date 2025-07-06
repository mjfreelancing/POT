using Pot.Data.Entities;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class IncomeRenewalCalculator : IIncomeRenewalCalculator
{
    public void Renew(IEnumerable<IncomeEntity> incomes, DateOnly todayDate)
    {
        foreach (var income in incomes)
        {
            var endDate = income.EndDate.GetValueOrDefault(DateOnly.MaxValue);

            if (todayDate >= endDate)
            {
                continue;
            }

            var nextDue = income.NextDue;

            while (nextDue < todayDate)
            {
                var days = income.Frequency.GetDaysToNext(income.NextDue, income.FrequencyCount);
                nextDue = income.NextDue.AddDays(days);

                // Don't advance beyond the end date
                if (nextDue <= endDate)
                {
                    income.NextDue = nextDue;
                }
            }
        }
    }
}
