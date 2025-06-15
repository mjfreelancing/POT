using Pot.App.Concerns.DependencyInjection;

namespace Pot.App.Features.Projections;

public interface IProjectionsService : IPotScopedDependency
{
    Task GetProjectionsAsync(CancellationToken cancellationToken);
}
