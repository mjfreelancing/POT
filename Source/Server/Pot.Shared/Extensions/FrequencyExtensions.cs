using Pot.Shared.Enumerations;

namespace Pot.Shared.Extensions;

public static class FrequencyExtensions
{
    private const double AverageDaysPerYear = 365.2425d;
    private const double AverageDaysPerMonth = AverageDaysPerYear / 12d;

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