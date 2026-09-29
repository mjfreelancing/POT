using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Incomes.Create.EntityChecks;

/// <summary>
/// Runs the checks that an income must satisfy before it is created.
/// </summary>
public interface IPreCreateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the supplied income can be created.
    /// </summary>
    /// <param name="incomeToCreate">The income pending creation.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null"/> when every
    /// check passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(IncomeEntity incomeToCreate, CancellationToken cancellationToken);
}
