using Pot.App.Errors;
using Pot.App.Features.Settings.Upsert.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.Upsert.EntityChecks;

/// <summary>
/// Validates that a setting can be created or updated before it is persisted.
/// </summary>
public interface IPreUpdateChecker : IPotScopedDependency
{
    /// <summary>
    /// Runs the registered pre-update checks for the supplied setting and reports the first failure.
    /// </summary>
    /// <remarks>
    /// Checks are evaluated in sequence and stop at the first failure, so any checks later in the chain are not
    /// executed once an earlier check fails.
    /// </remarks>
    /// <param name="input">The requested setting change.</param>
    /// <param name="settingToUpdate">
    /// The stored setting, or <see langword="null" /> when the setting does not exist yet.
    /// </param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> describing the first failed check, or <see langword="null" /> when every check
    /// passes.
    /// </returns>
    Task<ApiDetailError?> CanSaveAsync(Input input, SettingEntity? settingToUpdate, CancellationToken cancellationToken);
}
