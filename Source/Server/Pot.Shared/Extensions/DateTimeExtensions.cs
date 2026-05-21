namespace Pot.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="DateTime"/>.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Returns a Unix millisecond timestamp suitable for use as an ETag value.
    /// </summary>
    /// <param name="dateTime">The date and time to convert. Converted to UTC before computing the timestamp.</param>
    /// <returns>The number of milliseconds elapsed since the Unix epoch (1970-01-01T00:00:00Z).</returns>
    public static long GetEtag(this DateTime dateTime)
    {
        var utc = dateTime.ToUniversalTime();
        var offset = new DateTimeOffset(utc);

        return offset.ToUnixTimeMilliseconds();
    }
}
