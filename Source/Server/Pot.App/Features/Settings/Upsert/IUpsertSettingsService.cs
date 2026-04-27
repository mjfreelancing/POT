using AllOverIt.Patterns.Result;
using Pot.App.Features.Settings.Upsert.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.Upsert;

public interface IUpsertSettingService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpsertSettingAsync(Input input, CancellationToken cancellationToken);
}
