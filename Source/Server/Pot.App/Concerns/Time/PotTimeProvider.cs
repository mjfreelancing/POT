using AllOverIt.Assertion;

namespace Pot.App.Concerns.Time;

internal sealed class PotTimeProvider : ITimeProvider
{
    private readonly IAppContext _appContext;

    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    public DateOnly GetUtcDateNow() => DateOnly.FromDateTime(GetUtcDateTimeNow());
    public DateTime GetUtcDateTimeNow() => GetUtcNowOffset(true);
    public DateOnly GetLocalDateNow() => DateOnly.FromDateTime(GetLocalDateTimeNow());
    public DateTime GetLocalDateTimeNow() => GetUtcNowOffset(false);

    public PotTimeProvider(IAppContext appContext)
    {
        _appContext = appContext.WhenNotNull();
    }

    private DateTime GetUtcNowOffset(bool utc)
    {
        var utcNow = TimeProvider.GetUtcNow();

        var dateTime = utc
            ? utcNow.DateTime
            : utcNow.ToOffset(_appContext.TimeZoneOffset).DateTime;

        return DateTime.SpecifyKind(dateTime, utc ? DateTimeKind.Utc : DateTimeKind.Local);
    }
}
