using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth.PasswordReset.Request.Models;
using Pot.App.Features.Otp;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Data.Repositories.Users;
using Pot.Shared;

namespace Pot.App.Features.Auth.PasswordReset.Request;

internal sealed class RequestPasswordResetService : IRequestPasswordResetService
{
    private const int OtpExpiryMinutes = 15;

    private readonly IPersistableOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public RequestPasswordResetService(IPersistableOtpRepository otpRepository, IUserRepository userRepository,
        IOtpService otpService, ITimeProvider timeProvider, ILogger<RequestPasswordResetService> logger)
    {
        _otpRepository = otpRepository.WhenNotNull();
        _userRepository = userRepository.WhenNotNull();
        _otpService = otpService;
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<string> RequestResetAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { input.Username, input.CorrelationId });

        // Pro-actively expire old requests in case the background job hasn't run recently
        await _otpService
            .UpdateExpiredRequestsAsync(OtpReason.PasswordReset, cancellationToken)
            .ConfigureAwait(false);

        // Always returning a success result even if there's an error - we don't want to give
        // potential attackers any information about the validity of the username or other issue.
        var referenceCode = OtpGenerator.Create();

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
                return referenceCode;   // We don't know this user - return a reference code in case it is a bad actor
            }

            // Invalidate any existing active OTPs for this user
            await InvalidateActiveOtpsAsync(input.Username, cancellationToken).ConfigureAwait(false);

            // Create (and persist) a new OTP
            AddNewOtp(user, referenceCode, input.CorrelationId);

            await _otpRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return referenceCode;
        }
    }

    private async Task InvalidateActiveOtpsAsync(string username, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating active OTPs for username '{Username}'", username);

        var activeOtps = await _otpRepository
            .GetActiveRequestsForUsernameAsync(OtpReason.PasswordReset, username, cancellationToken)
            .ConfigureAwait(false);

        foreach (var activeOtp in activeOtps)
        {
            activeOtp.Status = OtpStatus.Invalidated;
            _otpRepository.Update(activeOtp);
        }
    }

    private string AddNewOtp(UserEntity user, string referenceCode, string correlationId)
    {
        var otpCode = OtpGenerator.Create();
        var currentUtc = _timeProvider.GetUtcDateTimeNow();

        var otpEntity = new OneTimePasswordEntity
        {
            CorrelationId = correlationId,
            Username = user.Username,
            Email = user.Email,
            Reason = OtpReason.PasswordReset,
            RefCode = referenceCode,
            OtpCode = otpCode,
            Status = OtpStatus.Active,
            CreatedUtc = currentUtc,
            ExpiryUtc = currentUtc.AddMinutes(OtpExpiryMinutes),
            User = user
        };

        _otpRepository.Add(otpEntity);

        return referenceCode;
    }
}
