using Pot.App.Features.Expenses.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.GetAll;

/// <summary>
/// Retrieves all expenses for the current site.
/// </summary>
public interface IGetExpensesService : IPotScopedDependency
{
    /// <summary>
    /// Gets all expenses for the current site.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The expenses for the current site, each with its accrual detail.</returns>
    Task<Output[]> GetAllExpensesAsync(CancellationToken cancellationToken);
}
