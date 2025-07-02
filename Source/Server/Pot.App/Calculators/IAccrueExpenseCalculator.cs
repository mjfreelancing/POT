using Pot.Shared.DependencyInjection;

namespace Pot.App.Calculators;

public interface IAccrueExpenseCalculator : IPotScopedDependency
{
    Task AccrueExpensesAsync(Guid accountRowId, CancellationToken cancellationToken);
}
