using AllOverIt.Patterns.Result;
using Pot.App.Features.Auth.Signup.Request.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.Signup.Request;

/// <summary>
/// Requests a signup verification code for a new user.
/// </summary>
/// <remarks>
/// The request invalidates any active OTPs for the same username and reason, persists a new OTP, and queues a
/// signup email. When the username is already taken the request completes successfully with a username-taken
/// status and no email is sent.
/// </remarks>
public interface IRequestSignupService : IPotScopedDependency
{
    /// <summary>
    /// Creates a signup request for the input's username and email, sending the signup email when available.
    /// </summary>
    /// <param name="input">The username and email to sign up, and the correlation identifier to record.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> whose output carries the reference code the user must supply when
    /// verifying, or a username-taken status when the name is unavailable. Failures are reported through that
    /// status rather than as an error result.
    /// </returns>
    Task<EnrichedResult<Output>> RequestSignupAsync(Input input, CancellationToken cancellationToken);
}
