using Pot.App.Errors;
using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Update.EntityChecks;

/// <summary>
/// Validates that a site can be updated before the change is persisted.
/// </summary>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the registered pre-update checks for the supplied site and reports the first failure.
    /// </summary>
    /// <remarks>
    /// Checks are evaluated in sequence and stop at the first failure, so any checks later in the chain are not
    /// executed once an earlier check fails.
    /// </remarks>
    /// <param name="input">The requested site change.</param>
    /// <param name="siteToUpdate">The stored site being updated.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null" /> when every check
    /// passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(Input input, SiteEntity siteToUpdate, CancellationToken cancellationToken);
}
