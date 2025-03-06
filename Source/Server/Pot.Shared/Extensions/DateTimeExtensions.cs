namespace Pot.Shared.Extensions;

public static class DateTimeExtensions
{
    public static long GetEtag(this DateTime dateTime)
    {
        var utc = dateTime.ToUniversalTime();
        var offset = new DateTimeOffset(utc);

        return offset.ToUnixTimeMilliseconds();
    }
}
