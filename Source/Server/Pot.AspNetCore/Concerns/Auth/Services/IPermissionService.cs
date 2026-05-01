using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth.Services;

public interface IPermissionService : IPotScopedDependency
{
    Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken);
}
