using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Health;

/// <summary>
/// Polls a named health check until it reports a healthy status or the operation is cancelled.
/// </summary>
internal interface IServiceHealthPoller : IPotSingletonDependency
{
    /// <summary>
    /// Blocks asynchronously until the named health check reports
    /// <see cref="Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy"/>,
    /// polling at the interval specified by <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The health check name and polling interval to use.</param>
    /// <param name="cancellationToken">A token that cancels the polling loop.</param>
    Task WaitForHealthyAsync(ServiceHealthPollerOptions options, CancellationToken cancellationToken);
}
