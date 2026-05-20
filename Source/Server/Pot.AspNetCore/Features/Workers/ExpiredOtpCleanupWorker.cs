using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Pot.App.Concerns.Time;
using Pot.App.Concerns.Time.Extensions;
using Pot.App.Features.Otp;
using Pot.AspNetCore.Concerns.Health;

namespace Pot.AspNetCore.Features.Workers;

internal sealed class ExpiredOtpCleanupWorker : BackgroundWorker
{
    private const int DelayMinutes = 5;

    private static readonly ServiceHealthPollerOptions DatabaseHealthPollerOptions = new() { Name = "database" };

    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceHealthPoller _serviceHealthPoller;

    public ExpiredOtpCleanupWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
        IServiceScopeFactory scopeFactory, IServiceHealthPoller serviceHealthPoller)
        : base(applicationLifetime)
    {
        _timeProvider = timeProvider.WhenNotNull();
        _scopeFactory = scopeFactory.WhenNotNull();
        _serviceHealthPoller = serviceHealthPoller.WhenNotNull();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Will not throw
        await _serviceHealthPoller
            .WaitForHealthyAsync(DatabaseHealthPollerOptions, stoppingToken)
            .ConfigureAwait(false);

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
                logger.LogInformation("Next Expired OTP cleanup scheduled for {NextOtpCleanupTimeUtc:O} (UTC)", nextUtc);

                await _timeProvider.WaitUntilUtcAsync(nextUtc.Value, stoppingToken);
            }
        }
    }
}
