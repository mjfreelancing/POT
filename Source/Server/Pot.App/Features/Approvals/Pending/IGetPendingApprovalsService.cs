using Pot.App.Features.Approvals.Pending.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Approvals.Pending;

/// <summary>
/// Retrieves the users who are awaiting approval.
/// </summary>
/// <remarks>
/// The query ignores the standard user query filters so users in an approval state are returned regardless of
/// their site scope.
/// </remarks>
public interface IGetPendingApprovalsService : IPotScopedDependency
{
    /// <summary>
    /// Gets all users whose status is pending approval.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The pending users, ordered by username.</returns>
    Task<List<Output>> GetAllAsync(CancellationToken cancellationToken);
}
