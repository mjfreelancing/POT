using Pot.App.Errors;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accounts.Delete.EntityChecks;

/// <summary>
/// Validates that an account can be deleted.
/// </summary>
/// <remarks>
/// Checks are evaluated in sequence and evaluation stops at the first failure, so no more than one error is
/// returned for a single attempt.
/// </remarks>
internal interface IPreDeleteChecker : IPotScopedDependency
{
    /// <summary>
    /// Determines whether the account identified by <paramref name="accountId"/> can be deleted.
    /// </summary>
    /// <param name="accountId">The external <c>RowId</c> of the account that is about to be deleted.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null"/> when the account can be deleted.</returns>
    Task<ApiDetailError?> CanDeleteAsync(Guid accountId, CancellationToken cancellationToken);
}
