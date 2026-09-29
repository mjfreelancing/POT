using Pot.App.Features.Users.GetAll.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Users.GetAll;

/// <summary>
/// Retrieves users for the administrative and site-scoped views.
/// </summary>
public interface IGetAllUsersService : IPotScopedDependency
{
    /// <summary>
    /// Gets every enabled user that has the administrator role.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A list of enabled administrator users across all sites.</returns>
    Task<List<Output>> GetAllEnabledAdminsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets every user that belongs to the current site.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A list of users for the current site.</returns>
    Task<List<Output>> GetAllForCurrentSiteAsync(CancellationToken cancellationToken);
}
