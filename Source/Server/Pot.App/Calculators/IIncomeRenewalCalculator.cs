using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Calculators;

public interface IIncomeRenewalCalculator : IPotSingletonDependency
{
    void Renew(IEnumerable<IncomeEntity> incomes, DateOnly advanceUtilDate);
}
