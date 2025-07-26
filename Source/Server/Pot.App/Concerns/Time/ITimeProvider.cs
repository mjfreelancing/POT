using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Time;

public interface ITimeProvider : IPotSingletonDependency
{
    public DateTimeOffset GetUtcNow();
    public DateTimeOffset GetLocalNow();    // Based on the application context's timezone - will eventually be per user
}
