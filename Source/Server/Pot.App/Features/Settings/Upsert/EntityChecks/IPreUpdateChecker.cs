using Pot.App.Errors;
using Pot.App.Features.Settings.Upsert.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.Upsert.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(Input input, SettingEntity? settingToUpdate, CancellationToken cancellationToken);
}
