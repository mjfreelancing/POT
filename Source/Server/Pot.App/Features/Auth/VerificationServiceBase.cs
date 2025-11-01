using AllOverIt.Assertion;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Otp;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Shared.Enumerations;

namespace Pot.App.Features.Auth;

internal abstract class VerificationServiceBase
{
    protected const int TooManyAttemptsWaitMinutes = 5;
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

    protected abstract EnrichedResult GetInvalidOutput();
    protected abstract EnrichedResult GetExpiredOutput();
    protected abstract EnrichedResult GetTooManyAttemptsOutput();
    protected abstract Task<EnrichedResult> ProcessVerificationCodeMatchAsync(OneTimePasswordEntity mostRecentOtp, CancellationToken cancellationToken);

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
