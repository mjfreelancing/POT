using AllOverIt.Patterns.Result;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Approvals.UpdateStatus;

public interface IUpdateApprovalService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateUserApprovalAsync(Input input, CancellationToken cancellationToken);
}
