using AllOverIt.Patterns.Result;
using Pot.App.Features.Auth.Signup.Complete.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.Signup.Complete;

/// <summary>
/// Verifies a signup code and, on success, creates the new user account.
/// </summary>
/// <remarks>
/// Successful verification creates the user in the approval status, assigns the default admin role, and notifies
/// the supplied platform administrators. A username that was taken since the signup was requested is reported as
/// a username-taken outcome rather than an error.
/// </remarks>
public interface IVerifySignupService : IPotScopedDependency
{
    /// <summary>
    /// Verifies the signup code for the input's username and reference code, creating the user when it matches.
    /// </summary>
    /// <param name="input">The username, reference code, verification code, and platform administrators to notify.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> reporting the verification outcome. A failed verification is returned
    /// as a successful result whose output status is invalid, expired, too many attempts, or username taken,
    /// rather than as an error result.
    /// </returns>
    Task<EnrichedResult<Output>> VerifySignupAsync(Input input, CancellationToken cancellationToken);
}
