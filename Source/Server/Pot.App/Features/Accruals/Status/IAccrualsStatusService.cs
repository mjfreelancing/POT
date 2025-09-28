using AllOverIt.Patterns.Result;
using Pot.App.Features.Accruals.Status.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Accruals.Status;

public interface IAccrualsStatusService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> GetStatusAsync(Input input, CancellationToken cancellationToken);
}
