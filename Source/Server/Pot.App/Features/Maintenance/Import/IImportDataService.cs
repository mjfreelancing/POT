using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import;

public interface IImportDataService : IPotScopedDependency
{
    Task<EnrichedResult<int>> ImportAsync(string publicKey, Stream zipStream, CancellationToken cancellationToken);
}
