using Pot.App.Errors;
using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Expenses.Update.EntityChecks;

/// <summary>
/// Runs the checks that an expense must satisfy before it is updated.
/// </summary>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the supplied expense update can be saved.
    /// </summary>
    /// <param name="request">The expense values to apply.</param>
    /// <param name="expenseAccount">The account the expense belongs to.</param>
    /// <param name="expenseToUpdate">The persisted expense being updated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null"/> when every
    /// check passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(Input request, AccountEntity expenseAccount, ExpenseEntity expenseToUpdate, CancellationToken cancellationToken);
}
