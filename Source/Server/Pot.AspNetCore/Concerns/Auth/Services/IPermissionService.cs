using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth.Services;

/// <summary>Defines operations for retrieving a user's permissions.</summary>
public interface IPermissionService : IPotScopedDependency
{
    /// <summary>Retrieves the permission strings assigned to a user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken);
}
