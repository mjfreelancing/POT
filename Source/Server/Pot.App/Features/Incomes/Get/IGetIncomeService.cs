using Pot.App.Features.Incomes.Get.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Get;

/// <summary>
/// Retrieves an income for the current site.
/// </summary>
public interface IGetIncomeService : IPotScopedDependency
{
    /// <summary>
    /// Gets the income identified by the supplied identifier.
    /// </summary>
    /// <param name="incomeId">The identifier of the income to retrieve.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The income, or <see langword="null"/> when it does not exist.</returns>
    Task<Output?> GetIncomeAsync(Guid incomeId, CancellationToken cancellationToken);
}
