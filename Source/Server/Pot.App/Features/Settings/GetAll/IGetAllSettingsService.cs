using AllOverIt.Patterns.Result;
using Pot.App.Features.Settings.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Settings.GetAll;

public interface IGetAllSettingsService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> GetAllSettingsAsync(CancellationToken cancellationToken);
}
