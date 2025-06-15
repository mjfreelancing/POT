using Pot.App.Features.Projections.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Projections;

public interface IProjectionsService : IPotScopedDependency
{
    Task<Output> GetProjectionsAsync(ProjectionOptions options, CancellationToken cancellationToken);
}
