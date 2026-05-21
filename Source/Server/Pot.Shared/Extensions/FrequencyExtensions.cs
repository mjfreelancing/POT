using Pot.Shared.Enumerations;

namespace Pot.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="Frequency"/> that compute recurrence intervals.
/// </summary>
public static class FrequencyExtensions
{
    private const double AverageDaysPerYear = 365.2425d;
    private const double AverageDaysPerMonth = AverageDaysPerYear / 12d;

    /// <summary>
    /// Returns the exact number of days until the next occurrence of the given <paramref name="frequency"/> from <paramref name="fromDate"/>.
    /// </summary>
    /// <param name="frequency">The recurrence frequency.</param>
    /// <param name="fromDate">The date from which the next occurrence is calculated.</param>
    /// <param name="frequencyCount">The number of frequency units to advance.</param>
    /// <returns>The number of days from <paramref name="fromDate"/> to the next occurrence.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="frequency"/> is <see cref="Frequency.OneTime"/>, which has no next occurrence.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="frequency"/> is not a supported value.</exception>
    public static int GetDaysToNext(this Frequency frequency, DateOnly fromDate, int frequencyCount)
    {
        if (frequency == Frequency.Days)
        {
            return frequencyCount;
        }

        if (frequency == Frequency.Weeks)
        {
            return 7 * frequencyCount;
        }

        if (frequency == Frequency.Months)
        {
            var nextDate = fromDate.AddMonths(frequencyCount);
            return nextDate.DayNumber - fromDate.DayNumber;
        }

        if (frequency == Frequency.EndOfMonth)
        {
            var targetMonthDate = fromDate.AddMonths(frequencyCount);
            var lastDayInTargetMonth = DateTime.DaysInMonth(targetMonthDate.Year, targetMonthDate.Month);
            var nextDate = new DateOnly(targetMonthDate.Year, targetMonthDate.Month, lastDayInTargetMonth);

            return nextDate.DayNumber - fromDate.DayNumber;
        }

        if (frequency == Frequency.Years)
        {
            var nextDate = fromDate.AddYears(frequencyCount);
            return nextDate.DayNumber - fromDate.DayNumber;
        }

        if (frequency == Frequency.OneTime)
        {
            throw new InvalidOperationException("The 'OneTime' frequency does not have a next occurrence.");
        }

        throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null);
    }

    /// <summary>
    /// Returns the average number of days until the next occurrence of the given <paramref name="frequency"/>.
    /// </summary>
    /// <remarks>
    /// Uses the proleptic Gregorian average year length of 365.2425 days. For month-based frequencies this
    /// produces a calendar-averaged value rather than an exact calendar calculation.
    /// </remarks>
    /// <param name="frequency">The recurrence frequency.</param>
    /// <param name="frequencyCount">The number of frequency units to advance.</param>
    /// <returns>The average number of days until the next occurrence.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="frequency"/> is <see cref="Frequency.OneTime"/>, which has no recurring average period.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="frequency"/> is not a supported value.</exception>
    public static double GetAverageDaysToNext(this Frequency frequency, int frequencyCount)
    {
        if (frequency == Frequency.Days)
        {
            return frequencyCount;
        }

        if (frequency == Frequency.Weeks)
        {
            return 7d * frequencyCount;
        }

        if (frequency == Frequency.Months || frequency == Frequency.EndOfMonth)
        {
            return AverageDaysPerMonth * frequencyCount;
        }

        if (frequency == Frequency.Years)
        {
            return AverageDaysPerYear * frequencyCount;
        }

        if (frequency == Frequency.OneTime)
        {
            throw new InvalidOperationException($"The '{Frequency.OneTime.Name}' frequency does not have a recurring average period.");
        }

        throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null);
    }
}