using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.PasswordReset.Verify.Models;
using Pot.App.Features.Otp;
using Pot.Data.Repositories.Otp;
using Pot.Data.Repositories.Users;
using Pot.Shared;

namespace Pot.App.Features.Auth.PasswordReset.Verify;

internal sealed class VerifyPasswordResetService : IVerifyPasswordResetService
{
    private const int TooManyAttemptsWaitMinutes = 5;
    private const int MaxAttempts = 3;

    private static readonly EnrichedResult<Output> InvalidOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.Invalid,
        Message = "Invalid. Try again."     // Could be invalid code or username - not giving any hints
    });

    private static readonly EnrichedResult<Output> ExpiredOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.Expired,
        Message = "Expired. Send a new request."
    });

    private static readonly EnrichedResult<Output> TooManyAttemptsOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.TooManyAttempts,
        Message = "Too Many Attempts. Send a new request.",
        RetryMinutes = TooManyAttemptsWaitMinutes
    });

    private static readonly EnrichedResult<Output> SuccessOutput = EnrichedResult.Success(new Output
    {
        Status = OutputStatus.Success,
        Message = "Success"
    });

    private readonly IPersistableOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public VerifyPasswordResetService(IPersistableOtpRepository otpRepository, IUserRepository userRepository,
        IOtpService otpService, ITimeProvider timeProvider, ILogger<VerifyPasswordResetService> logger)
    {
        _otpRepository = otpRepository.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _otpService = otpService;
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> VerifyResetAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.ReferenceCode, input.VerificationCode });

        // Pro-actively expire old requests in case the background job hasn't run recently
        await _otpService
            .UpdateExpiredRequestsAsync(OtpReason.PasswordReset, cancellationToken)
            .ConfigureAwait(false);

        using (_otpRepository.WithTracking())
        {
            // Check the user exists and get their email - must be performed within the tracking block so that
            // OneTimePasswordEntity updates are persisted without error (due to FK).
            var user = await _userRepository
                .GetByUsernameAsync(input.Username, cancellationToken)
                .ConfigureAwait(false);

            if (user?.Email is null)
            {
                _logger.LogInformation("No user found for username '{Username}' or no associated email", input.Username);
                return InvalidOutput;
            }

            var output = await ProcessVerificationAsync(input.Username, input.ReferenceCode, input.VerificationCode, cancellationToken).ConfigureAwait(false);

            await _otpRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return output;
        }
    }

    private async Task<bool> IsUserRateLimitedAsync(string username, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking if the user '{Username}' is rate limited", username);

        var isRateLimited = await _otpService
            .HasReachedRateLimitAsync(OtpReason.PasswordReset, username, cancellationToken)
            .ConfigureAwait(false); ;

        if (isRateLimited)
        {
            _logger.LogInformation("Rate limit exceeded for username '{Username}'", username);
        }

        return isRateLimited;
    }

    private async Task<EnrichedResult<Output>> ProcessVerificationAsync(string username, string referenceCode, string verificationCode,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing verification for username '{Username}'", username);

        // Most likely will only be one, but there is a chance of duplicates
        var otpEntities = await _otpRepository
            .GetRequestsForUsernameAndRefCodeAsync(OtpReason.PasswordReset, username, referenceCode, cancellationToken)
            .ConfigureAwait(false);

        if (otpEntities.Count == 0)
        {
            // username + reference code not found for PasswordReset
            return InvalidOutput;
        }

        var mostRecentOtp = otpEntities
            .OrderByDescending(otp => otp.CreatedUtc)
            .First();

        if (mostRecentOtp.Status == OtpStatus.Expired)
        {
            return ExpiredOutput;
        }

        if (mostRecentOtp.Status == OtpStatus.Active && mostRecentOtp.OtpCode == verificationCode)
        {
            mostRecentOtp.Status = OtpStatus.Used;
            mostRecentOtp.VerifiedUtc = _timeProvider.GetUtcDateTimeNow();

            _otpRepository.Update(mostRecentOtp);

            return SuccessOutput;
        }

        // Requests for a new OTP will always create a new record so bad actors think all is good and to keep the
        // client-side logic simple. So, if the verification code was invalid we always check rate limiting
        // before further processing the verification.
        var isRateLimited = await IsUserRateLimitedAsync(username, cancellationToken).ConfigureAwait(false);

        if (isRateLimited)
        {
            return TooManyAttemptsOutput;
        }

        mostRecentOtp.AttemptCount++;

        if (mostRecentOtp.AttemptCount >= MaxAttempts)
        {
            mostRecentOtp.Status = OtpStatus.Failed;

            _otpRepository.Update(mostRecentOtp);

            return TooManyAttemptsOutput;
        }

        return InvalidOutput;
    }
}
