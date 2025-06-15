namespace Pot.Shared.Extensions;

public static class FrequencyExtensions
{
    public static int GetDays(this Frequency frequency, DateOnly fromDate, int frequencyCount)
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

        if (frequency == Frequency.Years)
        {
            var nextDate = fromDate.AddYears(frequencyCount);
            return nextDate.DayNumber - fromDate.DayNumber;
        }

        throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null);
    }
}