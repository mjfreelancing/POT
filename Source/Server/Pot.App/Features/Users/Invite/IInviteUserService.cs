using AllOverIt.Patterns.Result;
using Pot.App.Features.Users.Invite.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.Invite;

public interface IInviteUserService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> InviteUserAsync(Input input, CancellationToken cancellationToken);
}
