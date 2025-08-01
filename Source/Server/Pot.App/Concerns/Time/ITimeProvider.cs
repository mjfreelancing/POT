using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Time;

public interface ITimeProvider : IPotSingletonDependency
{
    public DateOnly GetUtcDateNow();
    public DateTime GetUtcDateTimeNow();

    // Based on the application context's timezone - will eventually be per user
    public DateOnly GetLocalDateNow();
    public DateTime GetLocalDateTimeNow();
}
