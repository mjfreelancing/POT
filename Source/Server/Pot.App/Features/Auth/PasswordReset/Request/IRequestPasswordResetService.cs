using Pot.App.Features.Auth.PasswordReset.Request.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.PasswordReset.Request;

/// <summary>
/// Requests a password reset for a user and issues the one-time data used to complete it.
/// </summary>
/// <remarks>
/// The request invalidates any active OTPs for the same username and reason, persists a new OTP, and queues a
/// change-password email. A reference code is always returned, even when the username is unknown, so callers
/// cannot tell whether an account exists.
/// </remarks>
public interface IRequestPasswordResetService : IPotScopedDependency
{
    /// <summary>
    /// Creates a password reset request for the input's username and sends the associated email.
    /// </summary>
    /// <param name="input">The username to reset and the correlation identifier to record with the request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// The reference code the user must supply when verifying the reset. An arbitrary code is returned when the
    /// username is unknown, so an unknown user cannot be distinguished from a valid request.
    /// </returns>
    Task<string> RequestResetAsync(Input input, CancellationToken cancellationToken);
}
