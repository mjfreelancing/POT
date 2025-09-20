using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.Me;

public interface IUserService : IPotScopedDependency
{
    Task<string?> GetUsernameAsync(Guid userId, CancellationToken cancellationToken);
}
