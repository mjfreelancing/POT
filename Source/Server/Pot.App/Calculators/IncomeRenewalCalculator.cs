using Pot.Data.Entities;
using Pot.Shared.Extensions;

namespace Pot.App.Calculators;

internal sealed class IncomeRenewalCalculator : IIncomeRenewalCalculator
{
    public void Renew(IEnumerable<IncomeEntity> incomes, DateOnly advanceUtilDate, bool creditAccount = false)
    {
        foreach (var income in incomes)
        {
            if (income.Frequency == Shared.Frequency.OneTime)
            {
                // One-time incomes do not renew
                continue;
            }

            var endDate = income.EndDate.GetValueOrDefault(DateOnly.MaxValue);

            if (advanceUtilDate >= endDate)
            {
                continue;
            }

            var nextDue = income.NextDue;

            // Do not process items due on the advanceUtilDate - theoretically 'still due' and it would affect how projections
            // are calculated because the expenses would continue to advance before the credit could be considered.
            while (nextDue < advanceUtilDate)
            {
                var days = income.Frequency.GetDaysToNext(income.NextDue, income.FrequencyCount);
                nextDue = income.NextDue.AddDays(days);

                if (nextDue <= endDate)
                {
                    if (creditAccount)
                    {
                        income.Account.Balance += income.Amount;
                    }

                    income.NextDue = nextDue;
                }
            }
        }
    }
}
