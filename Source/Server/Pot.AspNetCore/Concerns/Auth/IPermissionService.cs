using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth;

public interface IPermissionService : IPotScopedDependency
{
    Task<HashSet<string>> GetPermissionsAsync(Guid memberId);
}
