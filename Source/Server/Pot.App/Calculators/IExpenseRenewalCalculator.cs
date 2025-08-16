using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Calculators;

public interface IExpenseRenewalCalculator : IPotSingletonDependency
{
    void Renew(IEnumerable<ExpenseEntity> expenses, DateOnly advanceUntilDate);
}
