using AllOverIt.Assertion;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Otp;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Auth;

/// <summary>
/// Provides the shared OTP verification flow used by signup and password reset, covering expiry, rate-limiting,
/// and attempt tracking.
/// </summary>
/// <remarks>
/// Verification is stateful: pending requests are swept for expiry before each attempt, an unmatched code
/// increments the request's attempt count, and the request is failed once the maximum attempts are reached.
/// Derived services supply the outcome payloads and the action taken when a code matches.
/// </remarks>
internal abstract class VerificationServiceBase
{
    /// <summary>The number of minutes a caller must wait after exceeding the allowed verification attempts.</summary>
    protected const int TooManyAttemptsWaitMinutes = 5;

    /// <summary>The number of failed attempts permitted for a request before it is failed.</summary>
    protected const int MaxAttempts = 3;

    private readonly OtpReason _verifyReason;
    private readonly IOtpService _otpService;
    private readonly IOtpRepository _otpRepository;
    private readonly ILogger _logger;

    protected VerificationServiceBase(OtpReason verifyReason, IOtpService otpService, IOtpRepository otpRepository, ILogger logger)
    {
        _verifyReason = verifyReason;
        _otpRepository = otpRepository.WhenNotNull();
        _otpService = otpService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Creates the outcome returned when the supplied verification code is invalid.
    /// </summary>
    /// <returns>The outcome payload for an invalid code.</returns>
    protected abstract EnrichedResult GetInvalidOutput();

    /// <summary>
    /// Creates the outcome returned when the matching request has expired.
    /// </summary>
    /// <returns>The outcome payload for an expired request.</returns>
    protected abstract EnrichedResult GetExpiredOutput();

    /// <summary>
    /// Creates the outcome returned when the caller has exceeded the allowed verification attempts.
    /// </summary>
    /// <returns>The outcome payload for rate-limited verification.</returns>
    protected abstract EnrichedResult GetTooManyAttemptsOutput();

    /// <summary>
    /// Applies the action taken when the supplied code matches an active request.
    /// </summary>
    /// <param name="mostRecentOtp">The active request whose code matched.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The outcome produced once the matched code has been processed.</returns>
    protected abstract Task<EnrichedResult> ProcessVerificationCodeMatchAsync(OneTimePasswordEntity mostRecentOtp, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the supplied verification code against the most recent request and returns the outcome.
    /// </summary>
    /// <param name="username">The username the request belongs to.</param>
    /// <param name="referenceCode">The reference code identifying the request.</param>
    /// <param name="verificationCode">The code supplied by the caller.</param>
    /// <param name="onStatusUsed">An optional callback invoked when the matched request is marked as used.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The outcome of the verification attempt.</returns>
    /// <remarks>
    /// Pending requests are swept for expiry before the attempt is evaluated. A matching code is delegated to
    /// <see cref="ProcessVerificationCodeMatchAsync"/>; otherwise the attempt is counted and the caller is
    /// rate-limited once <see cref="MaxAttempts"/> is reached.
    /// </remarks>
    protected async Task<EnrichedResult> ProcessVerificationAsync(string username, string referenceCode, string verificationCode,
        Action<OneTimePasswordEntity> onStatusUsed, CancellationToken cancellationToken)
    {
        // Pro-actively expire old requests in case the background job hasn't run recently
        await _otpService
            .UpdateExpiredRequestsAsync(_verifyReason, cancellationToken)
            .ConfigureAwait(false);

        // Most likely will only be one, but there is a rare chance of duplicates
        var otpEntities = await _otpRepository
            .GetRequestsForUsernameAndRefCodeAsync(_verifyReason, username, referenceCode, cancellationToken)
            .ConfigureAwait(false);

        if (otpEntities.Count == 0)
        {
            // username + reference code not found for Signup
            return GetInvalidOutput();
        }

        var mostRecentOtp = otpEntities
            .OrderByDescending(otp => otp.CreatedUtc)
            .First();

        if (mostRecentOtp.Status == OtpStatus.Expired)
        {
            return GetExpiredOutput();
        }

        if (mostRecentOtp.Status == OtpStatus.Active && mostRecentOtp.OtpCode == verificationCode)
        {
            var result = await ProcessVerificationCodeMatchAsync(mostRecentOtp, cancellationToken);

            if (mostRecentOtp.Status == OtpStatus.Used)
            {
                onStatusUsed?.Invoke(mostRecentOtp);
            }

            return result;
        }

        // Requests for a new OTP will always create a new record so bad actors think all is good and to keep the
        // client-side logic simple. So, if the verification code was invalid we always check rate limiting
        // before further processing the verification.
        var isRateLimited = await IsUserRateLimitedAsync(mostRecentOtp.Username, cancellationToken).ConfigureAwait(false);

        if (isRateLimited)
        {
            return GetTooManyAttemptsOutput();
        }

        mostRecentOtp.AttemptCount++;

        if (mostRecentOtp.AttemptCount >= MaxAttempts)
        {
            mostRecentOtp.Status = OtpStatus.Failed;

            return GetTooManyAttemptsOutput();
        }

        return GetInvalidOutput();
    }

    private async Task<bool> IsUserRateLimitedAsync(string username, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking if the user '{Username}' is rate limited", username);

        var isRateLimited = await _otpService
            .HasReachedRateLimitAsync(_verifyReason, username, cancellationToken)
            .ConfigureAwait(false);

        if (isRateLimited)
        {
            _logger.LogInformation("Rate limit exceeded for username '{Username}'", username);
        }

        return isRateLimited;
    }
}
