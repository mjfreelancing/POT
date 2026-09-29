using Pot.App.Errors;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Update.EntityChecks;

/// <summary>
/// Validates that an account update can be saved.
/// </summary>
/// <remarks>
/// Checks are evaluated in sequence and evaluation stops at the first failure, so no more than one error is
/// returned for a single attempt.
/// </remarks>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the supplied update can be saved against the account it targets.
    /// </summary>
    /// <param name="input">The requested account changes.</param>
    /// <param name="accountToUpdate">The persisted account that will be modified.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the update can be saved.</returns>
    Task<ApiDetailError?> CanSaveAsync(Input input, AccountEntity accountToUpdate, CancellationToken cancellationToken);
}
