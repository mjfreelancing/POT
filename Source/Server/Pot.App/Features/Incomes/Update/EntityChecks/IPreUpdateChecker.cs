using Pot.App.Errors;
using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Update.EntityChecks;

/// <summary>
/// Runs the checks that an income must satisfy before it is updated.
/// </summary>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the supplied income update can be saved.
    /// </summary>
    /// <param name="request">The income values to apply.</param>
    /// <param name="incomeAccount">The account the income belongs to.</param>
    /// <param name="incomeToUpdate">The persisted income being updated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null"/> when every
    /// check passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(Input request, AccountEntity incomeAccount, IncomeEntity incomeToUpdate, CancellationToken cancellationToken);
}
