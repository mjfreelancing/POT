using AllOverIt.Assertion;

namespace Pot.App.Concerns.Time;

internal sealed class PotTimeProvider : ITimeProvider
{
    private readonly IAppContext _appContext;

    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;
    public DateTimeOffset GetUtcNow() => TimeProvider.GetUtcNow();

    public PotTimeProvider(IAppContext appContext)
    {
        _appContext = appContext.WhenNotNull();
    }

    public DateTimeOffset GetLocalNow()
    {
        return GetUtcNow().ToOffset(_appContext.TimeZoneOffset);
    }
}
