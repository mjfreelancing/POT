using Pot.App.Errors;
using Pot.App.Features.Users.Invite.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Invite.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ProblemDetailsError?> CanSaveAsync(Input input, CancellationToken cancellationToken);
}
