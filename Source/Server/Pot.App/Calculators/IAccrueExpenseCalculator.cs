using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Calculators;

public interface IAccrueExpenseCalculator : IPotScopedDependency
{
    Task AccrueExpensesAsync(AccountEntity account, CancellationToken cancellationToken);
}
