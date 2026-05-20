using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Pot.App.Concerns.Time;
using Pot.App.Concerns.Time.Extensions;
using Pot.App.Features.Notifications.BudgetReminder;
using Pot.App.Features.Users.GetAll;
using Pot.AspNetCore.Concerns.Health;
using Pot.Shared;

namespace Pot.AspNetCore.Features.Workers;

internal sealed class BudgetReminderEmailWorker : BackgroundWorker
{
    private static readonly ServiceHealthPollerOptions DatabaseHealthPollerOptions = new() { Name = "database" };

    private readonly ITimeProvider _timeProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceHealthPoller _serviceHealthPoller;

    public BudgetReminderEmailWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
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

                logger = serviceProvider.GetRequiredService<ILogger<BudgetReminderEmailWorker>>();

                try
                {
                    var allUsersService = serviceProvider.GetRequiredService<IGetAllUsersService>();

                    var allUsers = await allUsersService
                        .GetAllEnabledAdminsAsync(stoppingToken)
                        .ConfigureAwait(false);

                    foreach (var user in allUsers)
                    {
                        // We need a new scope for each user to ensure the site filtering is applied correctly
                        using var userScope = _scopeFactory.CreateScope();
                        var userServiceProvider = userScope.ServiceProvider;

                        var userContext = userServiceProvider.GetRequiredService<ICurrentUserContext>();
                        userContext.SetUserRowId(user.RowId);

                        var reminderService = userServiceProvider.GetRequiredService<IBudgetReminderService>();

                        await reminderService
                            .SendRemindersAsync(stoppingToken)
                            .ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "An error occurred during the reminder email process: {ExceptionMessage}", exception.Message);
                }

                var nextHour = _timeProvider.GetUtcDateTimeNow().AddHours(1);
                nextUtc = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 0, DateTimeKind.Utc);
            }

            if (nextUtc.HasValue)
            {
                logger.LogInformation("Next reminder email scheduled for {NextReminderEmailTimeUtc:O} (UTC)", nextUtc);

                await _timeProvider.WaitUntilUtcAsync(nextUtc.Value, stoppingToken);
            }
        }
    }
}
