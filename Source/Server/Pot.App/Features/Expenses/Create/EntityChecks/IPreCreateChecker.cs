using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Create.EntityChecks;

/// <summary>
/// Runs the checks that an expense must satisfy before it is created.
/// </summary>
public interface IPreCreateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the supplied expense can be created.
    /// </summary>
    /// <param name="expenseToCreate">The expense pending creation.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null"/> when every
    /// check passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(ExpenseEntity expenseToCreate, CancellationToken cancellationToken);
}
