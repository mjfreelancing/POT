using Pot.App.Features.Approvals.Pending.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Approvals.Pending;

public interface IGetPendingApprovalsService : IPotScopedDependency
{
    Task<List<Output>> GetAllAsync(CancellationToken cancellationToken);
}
