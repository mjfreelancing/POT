namespace Pot.AspNetCore.Concerns.Health;

/// <summary>
/// Configuration options for a <see cref="IServiceHealthPoller"/> polling operation.
/// </summary>
internal sealed record ServiceHealthPollerOptions
{
    /// <summary>The name of the registered health check to poll.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// The delay between consecutive health-check polls. Defaults to 2 seconds.
    /// </summary>
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(2);
}
