using AllOverIt.Patterns.Result;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.ResendInvite;

public interface IResendInviteService : IPotScopedDependency
{
    Task<EnrichedResult<bool>> ResendInviteAsync(Guid userRowId, CancellationToken cancellationToken);
}
