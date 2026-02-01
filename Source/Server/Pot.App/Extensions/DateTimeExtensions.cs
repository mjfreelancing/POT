namespace Pot.App.Extensions;

public static class DateTimeExtensions
{
    public static DateTime? ConvertToLocalDateTime(this DateTime? dateTimeUtc, IAppContext appContext)
    {
        if (!dateTimeUtc.HasValue)
        {
            return null;
        }

        return new DateTimeOffset(dateTimeUtc.Value).ToOffset(appContext.TimeZoneOffset).DateTime;
    }
}