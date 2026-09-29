using AllOverIt.Patterns.Result;
using Pot.App.Features.Incomes.Renew.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Renew;

/// <summary>
/// Renews one or more incomes for the current site.
/// </summary>
/// <remarks>
/// Renewal recalculates each income's schedule from the supplied mode and as-of date, and all changes are persisted
/// in a single save.
/// </remarks>
public interface IRenewIncomesService : IPotScopedDependency
{
    /// <summary>
    /// Applies renewal to the incomes identified by the input.
    /// </summary>
    /// <param name="input">The incomes to renew, the renewal mode, and the as-of date.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;bool&gt;</c> containing <see langword="true"/> when the incomes were renewed, or a
    /// failure when one or more of the supplied incomes do not exist.
    /// </returns>
    Task<EnrichedResult<bool>> RenewAsync(Input input, CancellationToken cancellationToken);
}
