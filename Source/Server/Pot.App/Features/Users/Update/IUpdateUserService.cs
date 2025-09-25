using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.Update.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Update;

public interface IUpdateUserService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> UpdateUserAsync(Input input, CancellationToken cancellationToken);
}
