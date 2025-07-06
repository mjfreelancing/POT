using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Calculators;

public interface IAccrueExpenseCalculator : IPotScopedDependency
{
    void AccrueExpenses(AccountEntity account, IEnumerable<ExpenseEntity> expenses, DateOnly? currentDate = null);
}
