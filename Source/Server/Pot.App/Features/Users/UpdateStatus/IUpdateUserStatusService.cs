using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.UpdateStatus;

public interface IUpdateUserStatusService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateUserStatusAsync(Input input, CancellationToken cancellationToken);
}
