using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Pot.App.Concerns.Time;
using Pot.App.Features.Otp;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Workers;

internal sealed class ExpiredOtpCleanupWorker : BackgroundWorker
{
    private const int DelayMinutes = 5;

    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExpiredOtpCleanupWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
        IServiceScopeFactory scopeFactory)
        : base(applicationLifetime)
    {
        _timeProvider = timeProvider.WhenNotNull();
        _scopeFactory = scopeFactory.WhenNotNull();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ILogger logger;
            DateTime? nextUtc = null;

            using (var scope = _scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                logger = serviceProvider.GetRequiredService<ILogger<ExpiredOtpCleanupWorker>>();

                try
                {
                    var otpService = serviceProvider.GetRequiredService<IOtpService>();

                    await otpService
                        .UpdateExpiredRequestsAsync(null, stoppingToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "An error occurred during the Expired OTP cleanup process: {ExceptionMessage}", exception.Message);
                }

                nextUtc = _timeProvider.GetUtcDateTimeNow().AddMinutes(DelayMinutes);
            }

            if (nextUtc.HasValue)
            {
                logger.LogInformation("Next Expired OTP cleanup scheduled for {NextBackupTimeUtc:O} (UTC)", nextUtc);

                await WaitUntilAsync(nextUtc.Value, stoppingToken);
            }
        }
    }

    private async Task WaitUntilAsync(DateTime targetUtc, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Checking more than once in case the system time changes or there is drift that may 
            // result in the next occurrence being a matter of seconds later rather than the expected
            // cron schedule (the latter has been observed).
            var currentUtc = _timeProvider.GetUtcDateTimeNow();

            var delayTimespan = targetUtc - currentUtc;

            if (delayTimespan <= TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(delayTimespan, cancellationToken);
        }
    }
}
