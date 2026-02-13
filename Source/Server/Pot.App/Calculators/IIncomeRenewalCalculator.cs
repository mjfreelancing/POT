using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;

namespace Pot.App.Calculators;

public interface IIncomeRenewalCalculator : IPotSingletonDependency
{
    void Renew(IEnumerable<IncomeEntity> incomes, RenewalMode mode, DateOnly asOfDate);
}
