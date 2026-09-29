using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Create.EntityChecks;

/// <summary>
/// Validates that an account can be created.
/// </summary>
/// <remarks>
/// Checks are evaluated in sequence and evaluation stops at the first failure, so no more than one error is
/// returned for a single attempt.
/// </remarks>
internal interface IPreCreateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the supplied account can be saved.
    /// </summary>
    /// <param name="accountToCreate">The account that is about to be created.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the account can be saved.</returns>
    Task<ApiDetailError?> CanSaveAsync(AccountEntity accountToCreate, CancellationToken cancellationToken);
}
