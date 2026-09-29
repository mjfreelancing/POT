using Pot.App.Features.Otp.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Otp;

/// <summary>
/// Creates and maintains the one-time passwords used to sign up, reset passwords, and verify identity.
/// </summary>
/// <remarks>
/// Adding OTP data invalidates any active requests for the same username and reason, then issues a fresh temporary
/// password and verification code. Expiry and rate-limiting are evaluated against the current UTC time.
/// </remarks>
public interface IOtpService : IPotScopedDependency
{
    /// <summary>
    /// Marks requests that have passed their expiry time as expired.
    /// </summary>
    /// <param name="reason">The OTP reason to expire, or <see langword="null"/> for all reasons.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The number of requests marked as expired.</returns>
    Task<int> UpdateExpiredRequestsAsync(OtpReason? reason, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the username has submitted too many failed verification attempts for the reason.
    /// </summary>
    /// <param name="reason">The OTP reason to check, such as signup or password reset.</param>
    /// <param name="username">The username whose failed attempts are counted.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// <see langword="true"/> when the failed attempt count has reached the rate limit; otherwise
    /// <see langword="false"/>.
    /// </returns>
    Task<bool> HasReachedRateLimitAsync(OtpReason reason, string username, CancellationToken cancellationToken);

    /// <summary>
    /// Creates and persists OTP data for a username that does not yet have a user account, as used by signup.
    /// </summary>
    /// <param name="reason">The OTP reason the request is being created for.</param>
    /// <param name="username">The username the verification codes will belong to.</param>
    /// <param name="email">The email address the codes are sent to.</param>
    /// <param name="correlationId">The correlation identifier recorded against the created request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The generated codes, expiry, and temporary password for the request.</returns>
    // Used for new signup
    Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, string username, string email, string correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates and persists OTP data for an existing user, as used by password reset and similar flows.
    /// </summary>
    /// <param name="reason">The OTP reason the request is being created for.</param>
    /// <param name="user">The existing user the request belongs to.</param>
    /// <param name="correlationId">The correlation identifier recorded against the created request.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The generated codes, expiry, and temporary password for the request.</returns>
    // Used for anything other than new signup, such as password reset (since there's an existing user)
    Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, UserEntity user, string correlationId, CancellationToken cancellationToken);
}
