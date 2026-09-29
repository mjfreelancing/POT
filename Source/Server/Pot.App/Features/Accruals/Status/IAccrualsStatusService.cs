using AllOverIt.Patterns.Result;
using Pot.App.Features.Accruals.Status.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accruals.Status;

/// <summary>
/// Reports which expense and income records require renewal, for a set of accounts and an as-of date.
/// </summary>
/// <remarks>
/// Renewal checks are read-only: they evaluate persisted schedules against the supplied as-of date and do not
/// mutate any state. Expense and income checks are performed sequentially so both can share a single
/// <c>DbContext</c> scope.
/// </remarks>
public interface IAccrualsStatusService : IPotScopedDependency
{
    /// <summary>
    /// Gets the renewal status for the accounts specified by the input, as of the input's as-of date.
    /// </summary>
    /// <param name="input">The accounts to evaluate and the date to evaluate them for.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the row identifiers of the expenses and incomes that
    /// require renewal.
    /// </returns>
    Task<EnrichedResult<Output>> GetStatusAsync(Input input, CancellationToken cancellationToken);
}
