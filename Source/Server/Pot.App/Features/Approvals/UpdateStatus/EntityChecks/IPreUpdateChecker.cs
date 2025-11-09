using Pot.App.Errors;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Approvals.UpdateStatus.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Input request, UserEntity user, CancellationToken cancellationToken);
}
