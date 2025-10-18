using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Time;
using Pot.Data.Repositories.Otp;
using Pot.Shared;

namespace Pot.App.Features.Otp;

internal sealed class OtpService : IOtpService
{
    private const int RateLimitMinutes = 5;
    private const int RateLimitMaxCount = 3;

    private readonly IPersistableOtpRepository _otpRepository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger _logger;

    public OtpService(IPersistableOtpRepository otpRepository, ITimeProvider timeProvider, ILogger<OtpService> logger)
    {
        _otpRepository = otpRepository.WhenNotNull();
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
}
