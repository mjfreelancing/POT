using Pot.App.Errors;
using Pot.App.Features.Users.Invite.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Invite.EntityChecks;

/// <summary>
/// Validates that a user can be invited before the invitation is processed.
/// </summary>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the registered pre-invite checks for the supplied input and reports the first failure.
    /// </summary>
    /// <remarks>
    /// Checks are evaluated in sequence and stop at the first failure, so any checks later in the chain are not
    /// executed once an earlier check fails.
    /// </remarks>
    /// <param name="input">The requested invitation.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null" /> when every check
    /// passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(Input input, CancellationToken cancellationToken);
}
