using Pot.App.Concerns.DependencyInjection;
using Pot.App.Features.Projections.Models;

namespace Pot.App.Features.Projections;

public interface IProjectionsService : IPotScopedDependency
{
    Task<Output> GetProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken);
}
