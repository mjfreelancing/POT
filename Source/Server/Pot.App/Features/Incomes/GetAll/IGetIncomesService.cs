using Pot.App.Features.Incomes.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.GetAll;

/// <summary>
/// Retrieves all incomes for the current site.
/// </summary>
public interface IGetIncomesService : IPotScopedDependency
{
    /// <summary>
    /// Gets all incomes for the current site.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The incomes for the current site.</returns>
    Task<Output[]> GetAllIncomesAsync(CancellationToken cancellationToken);
}
