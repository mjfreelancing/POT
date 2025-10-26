using AllOverIt.Patterns.Result;
using Pot.App.Features.Sites.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Update;

public interface IUpdateSiteService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateSiteAsync(Input input, CancellationToken cancellationToken);
}
