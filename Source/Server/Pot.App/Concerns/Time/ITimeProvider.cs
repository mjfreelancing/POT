using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Time;

public interface ITimeProvider : IPotSingletonDependency
{
    DateOnly GetUtcDateNow();
    DateTime GetUtcDateTimeNow();

    // Based on the application context's timezone - will eventually be per user
    DateOnly GetLocalDateNow();
    DateTime GetLocalDateTimeNow();

    TimeSpan GetLocalTimeZoneOffset();

    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
