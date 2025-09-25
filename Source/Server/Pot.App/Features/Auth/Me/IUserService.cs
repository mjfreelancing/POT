using Pot.App.Features.Auth.Me.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.Me;

public interface IUserService : IPotScopedDependency
{
    Task<Output?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken);
}
