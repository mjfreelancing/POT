using Pot.App.Features.Me.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Me;

public interface IUserService : IPotScopedDependency
{
    Task<Output?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken);
}
