using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;

namespace Pot.App.Calculators;

public interface IExpenseRenewalCalculator : IPotSingletonDependency
{
    void Renew(IEnumerable<ExpenseEntity> expenses, RenewalMode mode, DateOnly asOfDate);
}
