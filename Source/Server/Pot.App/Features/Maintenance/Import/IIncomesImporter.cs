using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Import;

public interface IIncomesImporter : IPotScopedDependency
{
    Task<int> ImportAsync(Stream zipStream, CancellationToken cancellationToken);
}
