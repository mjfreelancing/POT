using AllOverIt.Assertion;

namespace Pot.App.Concerns.Time;

internal sealed class PotTimeProvider : ITimeProvider
{
    private readonly IAppContext _appContext;

    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;
    public DateOnly GetUtcNow() => ToDateOnly(GetUtcNowOffset());

    public PotTimeProvider(IAppContext appContext)
    {
        _appContext = appContext.WhenNotNull();
    }

    public DateOnly GetLocalNow()
    {
        var dateTimeOffset = GetUtcNowOffset().ToOffset(_appContext.TimeZoneOffset);
        return ToDateOnly(dateTimeOffset);
    }

    private DateTimeOffset GetUtcNowOffset()
    {
        return TimeProvider.GetUtcNow();
    }

    private static DateOnly ToDateOnly(DateTimeOffset offset)
    {
        return DateOnly.FromDateTime(offset.Date);
    }
}
