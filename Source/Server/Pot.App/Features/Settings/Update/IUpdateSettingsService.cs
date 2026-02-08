using AllOverIt.Patterns.Result;
using Pot.App.Features.Settings.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.Update;

public interface IUpdateSettingService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateSettingAsync(Input input, CancellationToken cancellationToken);
}
