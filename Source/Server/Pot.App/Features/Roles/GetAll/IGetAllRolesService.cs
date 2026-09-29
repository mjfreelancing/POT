using Pot.App.Features.Roles.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Roles.GetAll;

/// <summary>
/// Retrieves the roles that can be assigned to a user.
/// </summary>
public interface IGetAllRolesService : IPotScopedDependency
{
    /// <summary>
    /// Gets all available roles.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The list of available roles.</returns>
    Task<List<Output>> GetAllRolesAsync(CancellationToken cancellationToken);
}
