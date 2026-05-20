using AllOverIt.Assertion;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pot.AspNetCore.Concerns.Health;

/// <summary>
/// Polls a specific <see cref="HealthCheckService"/> registration by name until it becomes healthy.
/// </summary>
internal sealed class ServiceHealthPoller : IServiceHealthPoller
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<ServiceHealthPoller> _logger;

    public ServiceHealthPoller(HealthCheckService healthCheckService, ILogger<ServiceHealthPoller> logger)
    {
        _healthCheckService = healthCheckService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task WaitForHealthyAsync(ServiceHealthPollerOptions options, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var report = await _healthCheckService
                .CheckHealthAsync(registration => registration.Name == options.Name, cancellationToken)
                .ConfigureAwait(false);

            if (report.Status == HealthStatus.Healthy)
            {
                return;
            }

            _logger.LogWarning("Service '{HealthCheckName}' is not yet ready. Waiting before retrying...", options.Name);

            await Task.Delay(options.PollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
