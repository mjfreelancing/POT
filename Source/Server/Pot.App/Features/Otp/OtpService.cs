using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Auth;
using Pot.App.Concerns.Time;
using Pot.App.Features.Auth;
using Pot.App.Features.Otp.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Shared;

namespace Pot.App.Features.Otp;

internal sealed class OtpService : IOtpService
{
    private const int RateLimitMinutes = 5;
    private const int RateLimitMaxCount = 3;
    private const int OtpExpiryMinutes = 15;
    private const int TempPasswordLenth = 12;

    private readonly IPersistableOtpRepository _otpRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public OtpService(IPersistableOtpRepository otpRepository, IUserPasswordHasher passwordHasher, ITimeProvider timeProvider,
        ILogger<OtpService> logger)
    {
        _otpRepository = otpRepository.WhenNotNull();
        _passwordHasher = passwordHasher.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<int> UpdateExpiredRequestsAsync(OtpReason? reason, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { reason });

        using (_otpRepository.WithTracking())
        {
            var currentUtc = _timeProvider.GetUtcDateTimeNow();

            // All types are expired if reason is null
            var pendingOtps = await _otpRepository
                .GetPendingExpiredAsync(reason, currentUtc, cancellationToken)
                .ConfigureAwait(false);

            foreach (var otp in pendingOtps)
            {
                otp.Status = OtpStatus.Expired;
            }

            var count = await _otpRepository.SaveAsync(cancellationToken);

            _logger.LogDebug("Expired OTP count: {ExpiredOtpCount}", count);

            return count;
        }
    }

    public async Task<bool> HasReachedRateLimitAsync(OtpReason reason, string username, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { reason, username });

        var currentUtc = _timeProvider.GetUtcDateTimeNow();
        var rateLimitedDateTime = currentUtc.AddMinutes(-RateLimitMinutes);

        var count = await _otpRepository
            .CountFailedRequestsForUsernameAsync(reason, username, rateLimitedDateTime, cancellationToken)
            .ConfigureAwait(false);

        return count >= RateLimitMaxCount;
    }

    public Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, string username, string email, string correlationId,
        CancellationToken cancellationToken)
    {
        return AddOtpDataForUserAsync(reason, null, username, email, correlationId, cancellationToken);
    }

    public Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, UserEntity user, string correlationId,
        CancellationToken cancellationToken)
    {
        return AddOtpDataForUserAsync(reason, user, user.Username, user.Email, correlationId, cancellationToken);
    }

    private async Task<UserOtpData> AddOtpDataForUserAsync(OtpReason reason, UserEntity? user, string username, string email,
        string correlationId, CancellationToken cancellationToken)
    {
        // Pro-actively expire old requests in case the background job hasn't run recently
        await UpdateExpiredRequestsAsync(reason, cancellationToken).ConfigureAwait(false);

        using (_otpRepository.WithTracking())
        {
            // Invalidate any existing active OTPs for this username (cannot use user entity as it may be null for sign up requests)
            await InvalidateActiveOtpsAsync(reason, username, cancellationToken).ConfigureAwait(false);

            var (tempPassword, otpEntity) = AddOtpData(reason, user, username, email, correlationId);

            await _otpRepository
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            return new UserOtpData
            {
                TempPassword = tempPassword,
                ReferenceCode = otpEntity.RefCode,
                OtpCode = otpEntity.OtpCode,
                OtpExpiryMinutes = OtpExpiryMinutes
            };
        }
    }

    private async Task InvalidateActiveOtpsAsync(OtpReason reason, string username, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating active OTPs for username '{Username}'", username);

        var activeOtps = await _otpRepository
            .GetActiveRequestsForUsernameAsync(reason, username, cancellationToken)
            .ConfigureAwait(false);

        foreach (var activeOtp in activeOtps)
        {
            activeOtp.Status = OtpStatus.Invalidated;
        }
    }

    // Cannot use username and email from user entity as it may be null for sign up requests
    private (string TempPassword, OneTimePasswordEntity OtpEntity) AddOtpData(OtpReason reason, UserEntity? user,
        string username, string email, string correlationId)
    {
        var currentUtc = _timeProvider.GetUtcDateTimeNow();
        var tempPassword = PasswordGenerator.Create(TempPasswordLenth);
        var tempPasswordHash = _passwordHasher.GetHash(user, tempPassword);
        var referenceCode = OtpGenerator.Create();
        var verificationCode = OtpGenerator.Create();

        var otpEntity = new OneTimePasswordEntity
        {
            CorrelationId = correlationId,
            Username = username,
            Email = email,
            Reason = reason,
            RefCode = referenceCode,
            OtpCode = verificationCode,
            TempPasswordHash = tempPasswordHash,
            Status = OtpStatus.Active,
            CreatedUtc = currentUtc,
            ExpiryUtc = currentUtc.AddMinutes(OtpExpiryMinutes),
            User = user // If not null, tracking is required for this user association otherwise EF will attempt to re-add the user
        };

        _otpRepository.Add(otpEntity);

        return (tempPassword, otpEntity);
    }
}
