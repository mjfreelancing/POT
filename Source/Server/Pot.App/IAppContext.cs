using Pot.Shared.DependencyInjection;

namespace Pot.App;

public interface IAppContext : IPotSingletonDependency
{
    // TODO: This will be replaced with a user-specific timezone offset in the future
    TimeSpan TimeZoneOffset { get; }
}
