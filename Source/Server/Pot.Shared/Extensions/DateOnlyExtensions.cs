namespace Pot.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="DateOnly"/>.
/// </summary>
public static class DateOnlyExtensions
{
    /// <summary>
    /// Returns the number of days from <paramref name="startDate"/> to <paramref name="endDate"/>.
    /// </summary>
    /// <param name="startDate">The date to calculate from.</param>
    /// <param name="endDate">The date to calculate to.</param>
    /// <returns>
    /// A positive value when <paramref name="endDate"/> is after <paramref name="startDate"/>,
    /// zero when they are equal, or a negative value when <paramref name="endDate"/> is before <paramref name="startDate"/>.
    /// </returns>
    public static int DaysUntil(this DateOnly startDate, DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber;
    }
}
