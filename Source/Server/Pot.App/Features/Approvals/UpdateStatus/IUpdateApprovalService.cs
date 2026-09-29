using AllOverIt.Patterns.Result;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Approvals.UpdateStatus;

/// <summary>
/// Approves or rejects a user that is awaiting approval.
/// </summary>
/// <remarks>
/// A successful update queues a status email to the user; delivery is handled asynchronously by the email
/// outbox.
/// </remarks>
public interface IUpdateApprovalService : IPotScopedDependency
{
    /// <summary>
    /// Applies the requested approval status to the user identified by the input's row identifier.
    /// </summary>
    /// <param name="input">The user identity, concurrency etag, and approval decision to apply.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> containing the updated user's row identifier and etag, or a
    /// failure when the user does not exist, is not awaiting approval, or has a stale etag.
    /// </returns>
    Task<EnrichedResult<Output>> UpdateUserApprovalAsync(Input input, CancellationToken cancellationToken);
}
