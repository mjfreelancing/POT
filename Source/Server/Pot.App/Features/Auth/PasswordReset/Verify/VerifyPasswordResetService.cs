using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.PasswordReset.Verify.Models;
using Pot.App.Features.Otp;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Data.Repositories.Users;
using Pot.Shared;

namespace Pot.App.Features.Auth.PasswordReset.Verify;

internal sealed class VerifyPasswordResetService : VerificationServiceBase, IVerifyPasswordResetService
{
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
    private readonly IPersistableUserRepository _userRepository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public VerifyPasswordResetService(IPersistableOtpRepository otpRepository, IPersistableUserRepository userRepository,
        IOtpService otpService, ITimeProvider timeProvider, ILogger<VerifyPasswordResetService> logger)
        : base(OtpReason.PasswordReset, otpService, otpRepository, logger)
    {
        _otpRepository = otpRepository.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<Output>> VerifyResetAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.ReferenceCode, input.VerificationCode });

        // _userRepository and _otpRepository share the same DbContext so only need to track / update one of them
        using var otpTracking = _otpRepository.WithTracking();

        // Check the username exists and get their email
        var user = await _userRepository
            .GetByUsernameOrDefaultAsync(input.Username, cancellationToken)
            .ConfigureAwait(false);

        if (user?.Email is null)
        {
            _logger.LogInformation("No user found for username '{Username}' or no associated email", input.Username);
            return InvalidOutput;
        }

        var verificationOutput = await ProcessVerificationAsync(
            input.Username,
            input.ReferenceCode,
            input.VerificationCode,
            otp =>
            {
                user.PasswordHash = otp.TempPasswordHash!;
            },
            cancellationToken).ConfigureAwait(false);

        var output = (EnrichedResult<Output>)verificationOutput;

        if (output.IsSuccess)
        {
            await _otpRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return output;
    }

    protected override EnrichedResult GetInvalidOutput()
    {
        return InvalidOutput;
    }

    protected override EnrichedResult GetExpiredOutput()
    {
        return ExpiredOutput;
    }

    protected override EnrichedResult GetTooManyAttemptsOutput()
    {
        return TooManyAttemptsOutput;
    }

    protected override async Task<EnrichedResult> ProcessVerificationCodeMatchAsync(OneTimePasswordEntity mostRecentOtp,
        CancellationToken cancellationToken)
    {
        mostRecentOtp.Status = OtpStatus.Used;
        mostRecentOtp.VerifiedUtc = _timeProvider.GetUtcDateTimeNow();

        return SuccessOutput;
    }
}